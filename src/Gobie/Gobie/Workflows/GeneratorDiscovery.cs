namespace Gobie.Workflows;

using Microsoft.CodeAnalysis;

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
                predicate: static (s, _) => s is ClassDeclarationSyntax,
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
        var (data, compilation) = (s.Item1, s.Item2);
        var diagnostics = new List<Diagnostic>();

        var model = compilation.GetSemanticModel(data.ClassDeclarationSyntax.SyntaxTree);
        var symbol = model.GetDeclaredSymbol(data.ClassDeclarationSyntax);

        if (symbol is null)
        {
            return new(diagnostics);
        }

        var templates = GetTemplates("GobieTemplate", (_, l, t) => new TemplateText(l, t), data.ClassDeclarationSyntax, compilation, diagnostics, ct);
        var templateDefs = AccumulateTemplates(templates, x => x, (_, x) => x, diagnostics, ct);

        var fileTemplates = GetTemplates("GobieFileTemplate", (f, l, t) => GetAttributeArgAndTemplate("GobieFileTemplateAttribute", f, l, t, compilation), data.ClassDeclarationSyntax, compilation, diagnostics, ct);
        var fileTemplateDefs = AccumulateTemplates(fileTemplates, x => x.Value.template, (d, t) => new UserFileTemplateData(d.Value.attArg, t), diagnostics, ct);

        var globalChildTemplates = GetTemplates("GobieGlobalChildTemplate", (f, l, t) => GetAttributeArgAndTemplate("GobieGlobalChildTemplateAttribute", f, l, t, compilation), data.ClassDeclarationSyntax, compilation, diagnostics, ct);
        var globalChildTemplateDefs = AccumulateTemplates(globalChildTemplates, x => x.Value.template, (d, t) => new GlobalChildTemplateData(d.Value.attArg, t), diagnostics, ct);

        var globalTemplates = GetTemplates("GobieGlobalFileTemplate", (f, l, t) => GetTwoAttributeArgsAndTemplate("GobieGlobalFileTemplateAttribute", f, l, t, compilation), data.ClassDeclarationSyntax, compilation, diagnostics, ct);
        var globalTemplateDefs = new List<GlobalTemplateData>();
        foreach (var template in globalTemplates.Where(x => x is not null))
        {
            ct.ThrowIfCancellationRequested();

            var res = Mustache.Parse(template!.Value.template.Text.AsSpan(), template!.Value.template.GetLocationAt);
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

                globalTemplateDefs.Add(new(template.Value.attArg1, template.Value.attArg2, t));
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

    /// <summary>
    /// This method **MUST** be used to get templates, because it handles reporting errors in the
    /// string type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="templateName"></param>
    /// <param name="templateBuilder">
    /// Generally, we expect we should always get a template out at the point the templateBuilder is
    /// called. However it is nullable because we can't guarentee it.
    /// </param>
    /// <param name="cds"></param>
    /// <param name="compliation"></param>
    /// <param name="diagnostics"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private static List<T> GetTemplates<T>(
        string templateName,
        Func<FieldDeclarationSyntax, LiteralExpressionSyntax, string, T?> templateBuilder,
        ClassDeclarationSyntax cds,
        Compilation compliation,
        List<Diagnostic> diagnostics,
        CancellationToken ct)
    {
        var templates = new List<T>();

        foreach (var field in cds.ChildNodes().OfType<FieldDeclarationSyntax>())
        {
            ct.ThrowIfCancellationRequested();

            foreach (AttributeSyntax att in field.AttributeLists.SelectMany(x => x.Attributes))
            {
                var a = ((IdentifierNameSyntax)att.Name).Identifier;
                if (a.Text == templateName)
                {
                    foreach (var variable in field.Declaration.Variables)
                    {
                        var model = compliation.GetSemanticModel(field.SyntaxTree);
                        var fieldSymbol = model.GetDeclaredSymbol(variable);
                        var eqSyntax = variable.ChildNodes().OfType<EqualsValueClauseSyntax>().FirstOrDefault();

                        if (eqSyntax.ChildNodes().OfType<BinaryExpressionSyntax>().FirstOrDefault() is BinaryExpressionSyntax bes)
                        {
                            diagnostics.Add(Diagnostic.Create(Diagnostics.TemplateIsConcatenatedString(), bes.GetLocation()));
                            goto DoneWithField;
                        }
                        else if (fieldSymbol is IFieldSymbol fs && fs.ConstantValue is not null && eqSyntax is not null)
                        {
                            if (eqSyntax.ChildNodes().OfType<InterpolatedStringExpressionSyntax>().FirstOrDefault() is InterpolatedStringExpressionSyntax i)
                            {
                                diagnostics.Add(Diagnostic.Create(Diagnostics.TemplateIsInterpolatedString(), i.GetLocation()));
                                goto DoneWithField;
                            }
                            else if (eqSyntax.ChildNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault() is LiteralExpressionSyntax l)
                            {
                                var outTemplate = templateBuilder(field, l, fs.ConstantValue.ToString());
                                if (outTemplate is not null)
                                {
                                    templates.Add(outTemplate);
                                }
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

    private static List<TResult> AccumulateTemplates<TData, TResult>(IEnumerable<TData> templates, Func<TData, TemplateText> selector, Func<TData, Mustache.TemplateDefinition, TResult> map, List<Diagnostic> diagnostics, CancellationToken ct)
    {
        var templateDefs = new List<TResult>();
        foreach (var template in templates)
        {
            ct.ThrowIfCancellationRequested();

            var tt = selector(template);
            var res = Mustache.Parse(tt.Text.AsSpan(), tt.GetLocationAt);
            if (res.Diagnostics is not null)
            {
                diagnostics?.AddRange(res.Diagnostics);
            }
            else if (res.Data is not null)
            {
                templateDefs.Add(map(template, res.Data));
            }
        }

        return templateDefs;
    }

    private static (string attArg, TemplateText template)? GetAttributeArgAndTemplate(string attributeName, FieldDeclarationSyntax f, LiteralExpressionSyntax l, string t, Compilation compilation)
    {
        foreach (var variable in f.Declaration.Variables)
        {
            var model = compilation.GetSemanticModel(f.SyntaxTree);
            var fieldSymbol = model.GetDeclaredSymbol(variable);

            if (fieldSymbol is IFieldSymbol fs && fs.ConstantValue is not null)
            {
                var ad = fieldSymbol.GetAttributes().First(x => x.AttributeClass.Name == attributeName);
                var fn = ad.ConstructorArguments[0].Value;
                return (fn.ToString(), new TemplateText(l, fs.ConstantValue.ToString()));
            }
        }

        return null;
    }

    private static (string attArg1, string attArg2, TemplateText template)? GetTwoAttributeArgsAndTemplate(string attributeName, FieldDeclarationSyntax f, LiteralExpressionSyntax l, string t, Compilation compilation)
    {
        foreach (var variable in f.Declaration.Variables)
        {
            var model = compilation.GetSemanticModel(f.SyntaxTree);
            var fieldSymbol = model.GetDeclaredSymbol(variable);

            if (fieldSymbol is IFieldSymbol fs && fs.ConstantValue is not null)
            {
                var ad = fieldSymbol.GetAttributes().First(x => x.AttributeClass.Name == attributeName);
                var a1 = ad.ConstructorArguments[0].Value;
                var a2 = ad.ConstructorArguments[1].Value;
                return (a1.ToString(), a2.ToString(), new TemplateText(l, fs.ConstantValue.ToString()));
            }
        }

        return null;
    }

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
