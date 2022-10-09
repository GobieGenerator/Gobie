namespace Gobie.Workflows;

public static class GeneratorDiscovery
{
    public static void GenerateAttributes(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<UserGeneratorAttributeData> userTemplateSyntax)
    {
        context.RegisterSourceOutput(
            userTemplateSyntax,
            static (spc, source) => BuildUserGeneratorAttributes(spc, source));
    }

    public static IncrementalValuesProvider<DataOrDiagnostics<UserGeneratorAttributeData>> FindUserTemplates(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsClassDeclaration(s),
                transform: static (ctx, ct) => GetUserTemplate(ctx, ct))
            .Where(static x => x is not null)!;
    }

    public static IncrementalValuesProvider<DataOrDiagnostics<UserGeneratorTemplateData>> GetFullGenerators(
        IncrementalValuesProvider<(UserGeneratorAttributeData Left, Compilation Right)> compliationAndGeneratorDeclarations)
    {
        return compliationAndGeneratorDeclarations.Select(static (s, ct) => GetFullTemplateDeclaration(s, ct));
    }

    private static DataOrDiagnostics<UserGeneratorTemplateData> GetFullTemplateDeclaration(
        (UserGeneratorAttributeData, Compilation) s,
        CancellationToken ct)
    {
        var (data, compliation) = (s.Item1, s.Item2);
        var diagnostics = new List<Diagnostic>();

        var model = compliation.GetSemanticModel(data.ClassDeclarationSyntax.SyntaxTree);
        var symbol = model.GetDeclaredSymbol(data.ClassDeclarationSyntax);

        if (symbol is null)
        {
            return new(diagnostics);
        }

        var templates = GetTemplates(data.ClassDeclarationSyntax, compliation, diagnostics);
        var templateDefs = new List<Mustache.TemplateDefinition>();
        foreach (var template in templates)
        {
            ct.ThrowIfCancellationRequested();

            var res = Mustache.Parse(template.Item1.AsSpan(), template.Item2);
            if (res.Diagnostics is not null)
            {
                diagnostics.AddRange(res.Diagnostics);
            }
            else if (res.Data is not null)
            {
                templateDefs.Add(res.Data);
            }
        }

        var globalChildTemplates = GetGlobalChildTemplates(data.ClassDeclarationSyntax, compliation);
        var globalChildTemplateDefs = new List<GlobalChildTemplateData>();
        foreach (var template in globalChildTemplates)
        {
            ct.ThrowIfCancellationRequested();

            var res = Mustache.Parse(template.template.AsSpan(), data.ClassDeclarationSyntax.GetLocation());
            if (res.Diagnostics is not null)
            {
                diagnostics.AddRange(res.Diagnostics);
            }
            else if (res.Data is not null)
            {
                globalChildTemplateDefs.Add(new(template.generatorName, res.Data));
            }
        }

        var globalTemplates = GetGlobalTemplates(data.ClassDeclarationSyntax, compliation, ct);
        var globalTemplateDefs = new List<GlobalTemplateData>();
        foreach (var template in globalTemplates)
        {
            ct.ThrowIfCancellationRequested();

            var res = Mustache.Parse(template.template.AsSpan(), data.ClassDeclarationSyntax.GetLocation());
            if (res.Diagnostics is not null)
            {
                diagnostics.AddRange(res.Diagnostics);
            }
            else if (res.Data is Mustache.TemplateDefinition t)
            {
                // Here we apply special rules to global templates. The only allowable option is to
                // have a single identifier node for ChildContent. Logical nodes are allowed if they
                // work off of ChildContent, but the use case isn't clear.
                if (t.Identifiers.Count > 1 ||
                    t.Identifiers.Count == 1 &&
                      ((t.Identifiers.First() != "ChildContent") ||
                       (t.Syntax.CountNodes(x => x.Type == Mustache.TemplateSyntaxType.Identifier) != 1)))
                {
                    diagnostics.Add(Diagnostic.Create(Diagnostics.GobieGlobalTemplateIdentifierIssue, null));
                    continue;
                }

                globalTemplateDefs.Add(new(template.generatorName, template.fileName, t));
            }
        }

        var fileTemplates = GetFileTemplates(data.ClassDeclarationSyntax, compliation);
        var fileTemplateDefs = new List<UserFileTemplateData>();
        foreach (var template in fileTemplates)
        {
            ct.ThrowIfCancellationRequested();

            var res = Mustache.Parse(template.template.AsSpan(), data.ClassDeclarationSyntax.GetLocation());
            if (res.Diagnostics is not null)
            {
                diagnostics.AddRange(res.Diagnostics);
            }
            else if (res.Data is not null)
            {
                fileTemplateDefs.Add(new(template.fileName, res.Data));
            }
        }

        if (diagnostics.Any())
        {
            return new(diagnostics);
        }

        var td = new UserGeneratorTemplateData(
                     data,
                     templateDefs,
                     fileTemplateDefs,
                     globalTemplateDefs,
                     globalChildTemplateDefs);

        if (td.HasAnyTemplate == false)
        {
            // Warn the user this won't do anything as is.
            diagnostics.Add(Diagnostic.Create(Diagnostics.UserTemplateIsEmpty(td.AttributeData.DefinitionIdentifier.ClassName), null));
        }

        return new(td, diagnostics);
    }

    private static List<(string, Location)> GetTemplates(ClassDeclarationSyntax cds, Compilation compliation, List<Diagnostic> diagnostics)
    {
        var templates = new List<(string, Location)>();

        foreach (var child in cds.ChildNodes())
        {
            if (child is FieldDeclarationSyntax f)
            {
                foreach (AttributeSyntax att in f.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var a = ((IdentifierNameSyntax)att.Name).Identifier;
                    if (a.Text == "GobieTemplate")
                    {
                        foreach (var variable in f.Declaration.Variables)
                        {
                            var model = compliation.GetSemanticModel(f.SyntaxTree);
                            var fieldSymbol = model.GetDeclaredSymbol(variable);
                            var eqSyntax = variable.ChildNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();

                            if (fieldSymbol is IFieldSymbol fs && fs.ConstantValue is not null && eqSyntax is not null)
                            {
                                if (eqSyntax.ChildNodes().OfType<InterpolatedStringExpressionSyntax>().FirstOrDefault() is InterpolatedStringExpressionSyntax i)
                                {
                                    diagnostics.Add(
                                        Diagnostic.Create(
                                            Diagnostics.TemplateIsInterpolatedString(),
                                            i.GetLocation()));
                                }
                                else if (eqSyntax.ChildNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault() is LiteralExpressionSyntax l)
                                {
                                    templates.Add((fs.ConstantValue.ToString(), l.GetLocation()));
                                    goto DoneWithField;
                                }
                            }
                        }
                    }
                }
            }

        DoneWithField:;
        }

        return templates;
    }

    private static List<(string fileName, string template)> GetFileTemplates(ClassDeclarationSyntax cds, Compilation compliation)
    {
        var templates = new List<(string fileName, string template)>();

        foreach (var child in cds.ChildNodes())
        {
            if (child is FieldDeclarationSyntax f)
            {
                foreach (AttributeSyntax att in f.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var a = ((IdentifierNameSyntax)att.Name).Identifier;
                    if (a.Text == "GobieFileTemplate")
                    {
                        foreach (var variable in f.Declaration.Variables)
                        {
                            var model = compliation.GetSemanticModel(f.SyntaxTree);
                            var fieldSymbol = model.GetDeclaredSymbol(variable);

                            if (fieldSymbol is IFieldSymbol fs && fs.ConstantValue is not null)
                            {
                                var ad = fieldSymbol.GetAttributes().First(x => x.AttributeClass.Name == "GobieFileTemplateAttribute");
                                var fn = ad.ConstructorArguments[0].Value;
                                templates.Add((fn.ToString(), fs.ConstantValue.ToString()));
                                goto DoneWithField;
                            }
                        }
                    }
                }
            }

        DoneWithField:;
        }

        return templates;
    }

    private static List<(string generatorName, string template)> GetGlobalChildTemplates(ClassDeclarationSyntax cds, Compilation compliation)
    {
        var templates = new List<(string generatorName, string template)>();

        foreach (var child in cds.ChildNodes())
        {
            if (child is FieldDeclarationSyntax f)
            {
                foreach (AttributeSyntax att in f.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var a = ((IdentifierNameSyntax)att.Name).Identifier;
                    if (a.Text == "GobieGlobalChildTemplate")
                    {
                        foreach (var variable in f.Declaration.Variables)
                        {
                            var model = compliation.GetSemanticModel(f.SyntaxTree);
                            var fieldSymbol = model.GetDeclaredSymbol(variable);

                            if (fieldSymbol is IFieldSymbol fs && fs.ConstantValue is not null)
                            {
                                var ad = fieldSymbol.GetAttributes().First(x => x.AttributeClass.Name == "GobieGlobalChildTemplateAttribute");
                                var fn = ad.ConstructorArguments[0].Value;
                                templates.Add((fn.ToString(), fs.ConstantValue.ToString()));
                                goto DoneWithField;
                            }
                        }
                    }
                }
            }

        DoneWithField:;
        }

        return templates;
    }

    private static List<(string generatorName, string fileName, string template)> GetGlobalTemplates(
        ClassDeclarationSyntax cds,
        Compilation compliation,
        CancellationToken ct)
    {
        var templates = new List<(string generatorName, string fileName, string template)>();

        foreach (var child in cds.ChildNodes())
        {
            ct.ThrowIfCancellationRequested();

            if (child is FieldDeclarationSyntax f)
            {
                foreach (AttributeSyntax att in f.AttributeLists.SelectMany(x => x.Attributes))
                {
                    var a = ((IdentifierNameSyntax)att.Name).Identifier;
                    if (a.Text == "GobieGlobalFileTemplate")
                    {
                        foreach (var variable in f.Declaration.Variables)
                        {
                            var model = compliation.GetSemanticModel(f.SyntaxTree);
                            var fieldSymbol = model.GetDeclaredSymbol(variable);

                            if (fieldSymbol is IFieldSymbol fs && fs.ConstantValue is not null)
                            {
                                var awre = fieldSymbol.GetAttributes();
                                var ad = fieldSymbol.GetAttributes().First(x => x.AttributeClass.Name == "GobieGlobalFileTemplateAttribute");
                                var generatorName = ad.ConstructorArguments[0].Value;
                                var fn = ad.ConstructorArguments[1].Value;
                                templates.Add((generatorName.ToString(), fn.ToString(), fs.ConstantValue.ToString()));
                                goto DoneWithField;
                            }
                        }
                    }
                }
            }

        DoneWithField:;
        }

        return templates;
    }

    private static bool IsClassDeclaration(SyntaxNode node) => node is ClassDeclarationSyntax;

    private static IEnumerable<Diagnostic> Duplicates(IEnumerable<RequiredParameter> requiredParameters)
    {
        var diagnostics = new List<Diagnostic>();

        foreach (var requestedOrders in requiredParameters.GroupBy(x => x.RequestedOrder).Where(x => x.Key != int.MaxValue))
        {
            foreach (var req in requestedOrders.AsEnumerable().Skip(1))
            {
                diagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.PriorityAlreadyDeclared(req.RequestedOrder),
                        req.RequestedOrderLocation));
            }
        }

        return diagnostics;
    }

    private static DataOrDiagnostics<UserGeneratorAttributeData>? GetUserTemplate(
        GeneratorSyntaxContext context,
        CancellationToken ct)
    {
        var cds = (ClassDeclarationSyntax)context.Node;
        var classLocation = cds.Identifier.GetLocation();

        if (cds.BaseList is null)
        {
            return null;
        }

        // Because we control the list of base types they can use this should be a very good though
        // imperfect filter we can run on the syntax alone.
        var gobieBaseTypeName = cds.BaseList.Types.SingleOrDefault(x => Config.GenToAttribute.ContainsKey(x.ToString()));
        if (gobieBaseTypeName is null)
        {
            return null;
        }

        //! We accumulate data here.
        var ident = new ClassIdentifier("Gobie", cds.Identifier.ToString());
        var genData = new UserGeneratorAttributeData(ident, cds, Config.GenToAttribute[gobieBaseTypeName.ToString()]);

        var diagnostics = new List<Diagnostic>();
        if (cds.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
        {
            diagnostics.Add(Diagnostic.Create(Diagnostics.UserTemplateIsPartial, classLocation));
        }

        if (cds.Modifiers.Any(x => x.IsKind(SyntaxKind.SealedKeyword)) == false)
        {
            diagnostics.Add(Diagnostic.Create(Diagnostics.UserTemplateIsNotSealed, classLocation));
        }

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);

        var invalidName = !cds.Identifier.ToString().EndsWith("Generator", StringComparison.OrdinalIgnoreCase);
        foreach (var attribute in classSymbol!.GetAttributes())
        {
            ct.ThrowIfCancellationRequested();

            var b = attribute?.AttributeClass?.ToString();
            if (attribute?.AttributeClass?.ToString() == "Gobie.GobieGeneratorNameAttribute")
            {
                if (attribute!.ConstructorArguments.Count() == 0)
                {
                    continue;
                }

                var genName = attribute!.ConstructorArguments[0].Value!.ToString();

                string? namespaceName = null;
                var namespaceVal = attribute.NamedArguments.SingleOrDefault(x => x.Key == "Namespace").Value;

                if (namespaceVal.IsNull == false)
                {
                    namespaceName = namespaceVal.Value!.ToString();
                }

                genData.WithName(genName!, namespaceName);
                invalidName = false;
                break;
            }
        }

        if (invalidName)
        {
            diagnostics.Add(Diagnostic.Create(Diagnostics.GeneratorNameInvalid, classLocation));
        }

        //! Diagnostics before here are errors that stop generation.
        if (diagnostics.Any())
        {
            return new(diagnostics);
        }

        var requiredPropertyNumber = 1;
        foreach (PropertyDeclarationSyntax node in cds.ChildNodes().Where(x => x is PropertyDeclarationSyntax))
        {
            ct.ThrowIfCancellationRequested();

            if (ConstantTypes.IsAllowedConstantType(node.Type, out var propertyType) == false)
            {
                // We don't need to break the whole template when they do this wrong.
                diagnostics.Add(Diagnostic.Create(Diagnostics.DisallowedTemplateParameterType("TODO"), node.Type.GetLocation()));
                continue;
            }

            var propertyInitalizer = string.Empty;
            if (node.Initializer is not null && node.Initializer.Value is LiteralExpressionSyntax les)
            {
                propertyInitalizer = les.Token.Text;
            }

            var propertySymbol = context.SemanticModel.GetDeclaredSymbol(node);
            if (propertySymbol is null)
            {
                // TODO is this a problem?
                continue;
            }

            foreach (var att in propertySymbol.GetAttributes())
            {
                if (att?.AttributeClass?.ToString() == "Gobie.Required")
                {
                    // Get the requested order if one was provided. Or give it the maximum so it
                    // goes at the end. Within values at the end they go in the order they were defined.
                    var order = int.MaxValue;
                    if (att.ConstructorArguments.Length > 0)
                    {
                        if (att.ConstructorArguments[0].Value is int o)
                        {
                            order = o;
                        }
                        else
                        {
                            // Here some arg exists but it isn't an int so the compiler should be
                            // erroring. So we just return diagnostics if any and stop.
                            return new(diagnostics);
                        }
                    }

                    genData.AddRequiredParameter(
                        new RequiredParameter(
                            order,
                            node.GetLocation(),
                            requiredPropertyNumber,
                            node.Identifier.Text,
                            propertyType,
                            propertyInitalizer));

                    requiredPropertyNumber++;

                    goto RequiredPropertyHandeled;
                }
            }

            // If we get here it isn't a required property, so we setup the optional one
            genData.OptionalParameters.Add(
                new OptionalParameter(
                            node.Identifier.Text,
                            propertyType,
                            propertyInitalizer));
        RequiredPropertyHandeled:;
        }

        diagnostics.AddRange(Duplicates(genData.RequiredParameters));

        return new(genData, diagnostics);
    }

    private static void BuildUserGeneratorAttributes(SourceProductionContext spc, UserGeneratorAttributeData data)
    {
        var generatedCode = @$"

            namespace {data.AttributeIdentifier.NamespaceName}
            {{
                /// <summary> This attribute will cause the generator defined by this thing here to
                /// run <see cref=""{data.DefinitionIdentifier.FullName}""/> to run. </summary>
                public sealed class {data.AttributeIdentifier.ClassName} : {data.AttributeBase}
                {{
                    public {data.AttributeIdentifier.ClassName}({string.Join(", ", data.RequiredParameters.Select(x => x.CtorArgumentString))})
                    {{
                        {string.Join(Environment.NewLine, data.RequiredParameters.Select(x => x.CtorAssignmentString))}
                    }}

                    {string.Join(Environment.NewLine, data.RequiredParameters.Select(x => x.PropertyString))}

                    {string.Join(Environment.NewLine, data.OptionalParameters.Select(x => x.PropertyString))}
                }}
            }}
            ";

        generatedCode = CSharpSyntaxTree.ParseText(generatedCode).GetRoot().NormalizeWhitespace().ToFullString();
        spc.AddSource($"_{data.AttributeIdentifier.FullName}.g.cs", generatedCode);
    }
}
