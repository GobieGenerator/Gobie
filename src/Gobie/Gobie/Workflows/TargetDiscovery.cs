namespace Gobie.Workflows;

public class TargetDiscovery
{
    private const string ClassName = "ClassName";
    private const string ClassNamespace = "ClassNamespace";

    public static IncrementalValuesProvider<MemberDeclarationSyntax> FindMembersWithAttributes(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsTargetForGeneration(s),
                transform: static (ctx, _) => (MemberDeclarationSyntax)ctx.Node)
            .Where(static x => x is not null)!;
    }

    /// <summary>
    /// Tries to find targets without relying on any complex operations.
    /// </summary>
    public static IncrementalValuesProvider<(T, ImmutableArray<UserGeneratorTemplateData>)?> FindProbableTargets<T>(
        IncrementalValuesProvider<(T Left, ImmutableArray<UserGeneratorTemplateData> Right)> mdsAndGenerators)
        where T : MemberDeclarationSyntax
    {
        return mdsAndGenerators.Select(selector: static (s, _) => FindProbableTargets(s.Left, s.Right));
    }

    public static IncrementalValuesProvider<DataOrDiagnostics<ImmutableArray<MemberTargetAndTemplateData>>> GetTargetsOrDiagnostics(
        IncrementalValuesProvider<((MemberDeclarationSyntax, ImmutableArray<UserGeneratorTemplateData>)? Left, Compilation Right)> data)
    {
        return data
            .Select(selector: static (s, _) =>
                GetTargetsOrDiagnostics(s.Left?.Item1, s.Left?.Item2, s.Right));
    }

    public static IncrementalValuesProvider<(AttributeSyntax, ImmutableArray<UserGeneratorTemplateData>)?> FindProbableAssemblyTargets(
        IncrementalValuesProvider<(AttributeSyntax Left, ImmutableArray<UserGeneratorTemplateData> Right)> assemblyAttributesAndGenerators)
    {
        return assemblyAttributesAndGenerators.Select(selector: static (s, _) => FindProbableAssemblyTargets(s.Left, s.Right));
    }

    public static IncrementalValuesProvider<DataOrDiagnostics<AssemblyTargetAndTemplateData>> GetAssemblyTargetsOrDiagnostics(
        IncrementalValuesProvider<((AttributeSyntax, ImmutableArray<UserGeneratorTemplateData>)? Left, Compilation Right)> data)
    {
        return data
            .Select(selector: static (s, _) =>
                GetAssemblyTargetsOrDiagnostics(s.Left?.Item1, s.Left?.Item2, s.Right));
    }

    private static DataOrDiagnostics<AssemblyTargetAndTemplateData> GetAssemblyTargetsOrDiagnostics(
        AttributeSyntax? attributeSyntax,
        ImmutableArray<UserGeneratorTemplateData>? templates,
        Compilation compilation)
    {
        var d = new List<Diagnostic>();

        var attName = attributeSyntax.Name.ToFullString();
        var ctypeName = attName + (attName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) ? "" : "Attribute");

        // TODO diagnostics if there are more than one template or similar.

        foreach (var template in templates)
        {
            if (template.GlobalTemplate.Count == 0) continue;

            if (ctypeName == template.AttributeData.AttributeIdentifier.ClassName)
            {
                var at = new AssemblyTargetAndTemplateData(template.AttributeData.DefinitionIdentifier.ClassName, template.GlobalTemplate[0].Template);
                return new(at);
            }
        }

        return new(d);

        ////throw new NotImplementedException();
        ////var at = new AssemblyTargetAndTemplateData()
    }

    private static (AttributeSyntax, ImmutableArray<UserGeneratorTemplateData>)? FindProbableAssemblyTargets(
        AttributeSyntax attributeSyntax,
        ImmutableArray<UserGeneratorTemplateData> right)
    {
        // Todo add logic
        return (attributeSyntax, right);
    }

    private static DataOrDiagnostics<ImmutableArray<MemberTargetAndTemplateData>> GetTargetsOrDiagnostics(
            MemberDeclarationSyntax? mds,
        ImmutableArray<UserGeneratorTemplateData>? templates,
        Compilation compilation)
    {
        var output = new List<MemberTargetAndTemplateData>();
        var diagnostics = new List<Diagnostic>();

        if (mds is null || templates is null)
        {
            return new DataOrDiagnostics<ImmutableArray<MemberTargetAndTemplateData>>(diagnostics);
        }

        // Initial data which would be the same for all generators operating on the member data
        // syntax. Note that for some syntax like fields, we can have multiple generation targets in
        // a single syntax node.
        ImmutableList<ImmutableDictionary<string, Mustache.RenderData>>? syntaxData = null;
        SemanticModel? semanticModel = null;

        // It looks like we aren't able to get the attribute args off of SourceAttributeData (from
        // typeInfo.GetAttributes()). This doesn't seem to be isolated to unit testing. Even in the
        // console client we find zero args when we follow the same process we use to get args for
        // required position or generator name. My current guess now is that the semantic model
        // above doesn't (and maybe cannot) have the definitions of the attributes we create in the
        // generator. (I'm assuming the register post generation initialization code is doing
        // something different, because we were able to get the constructor args for those).
        // Additionally I noticed that the SourceAttributeData (att) is missing the namespace and
        // doesn't say attribute at the end. So this seems like what happened when the unit tests
        // were missing a reference.
        foreach (var att in mds.AttributeLists.SelectMany(x => x.Attributes))
        {
            // TODO, maybe we should test that we can't resolve the specific attribute details and
            // then look to the syntax? I wonder if we define a generator in one lib and use it in
            // another whether that is even viable. And in that case we might be able to see the
            // full class when the dependant lib compiles.
            var attName = att.Name.ToFullString();
            var ctypeName = attName + (attName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) ? "" : "Attribute");

            foreach (var template in templates)
            {
                if (ctypeName == template.AttributeData.AttributeIdentifier.ClassName)
                {
                    // First thing we do, now that we know we have work to do, is to initialize data
                    // we will use for every template we are generating.
                    semanticModel ??= compilation.GetSemanticModel(mds.SyntaxTree);
                    syntaxData ??= GetSyntaxData(semanticModel, mds);

                    var attributeData = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();

                    if (att.ArgumentList is not null)
                    {
                        for (int i = 0; i < att.ArgumentList.Arguments.Count; i++)
                        {
                            var arg = att.ArgumentList.Arguments[i];
                            var constValArg = semanticModel.GetConstantValue(arg.Expression);
                            if (arg.NameEquals is null && constValArg.HasValue && i < template.AttributeData.RequiredParameters.Count())
                            {
                                // This is a required argument either with or without colon equals
                                var ident = template.AttributeData.RequiredParameters.ElementAt(i).NamePascal;

                                if (arg.Expression.Kind() == SyntaxKind.NullLiteralExpression)
                                {
                                    attributeData.Add(new Mustache.RenderData(ident, string.Empty, false));
                                }
                                else
                                {
                                    attributeData.Add(new Mustache.RenderData(ident, constValArg.Value!.ToString(), true));
                                }
                            }
                            else if (arg.NameEquals is not null && constValArg.HasValue)
                            {
                                // Named parameter (i.e. optional value prefixed by 'Name
                                // = '
                                var n = arg.NameEquals.Name.ToFullString().Trim();
                                if (arg.Expression.Kind() == SyntaxKind.NullLiteralExpression)
                                {
                                    attributeData.Add(new Mustache.RenderData(n, string.Empty, false));
                                }
                                else
                                {
                                    attributeData.Add(new Mustache.RenderData(n, constValArg.Value!.ToString(), true));
                                }
                            }
                        }

                        if (attributeData.Count < template.AttributeData.RequiredParameters.Count())
                        {
                            for (int i = attributeData.Count; i < template.AttributeData.RequiredParameters.Count(); i++)
                            {
                                var param = template.AttributeData.RequiredParameters.ElementAt(i);
                                var ident = param.NamePascal;
                                attributeData.Add(
                                    ident,
                                    new Mustache.RenderData(ident, param.InitalizerLiteral, true));
                            }
                        }
                    }

                    var attributeDataImmutable = attributeData.ToImmutable();

                    foreach (var target in syntaxData)
                    {
                        var sb = new StringBuilder();
                        var intersections = target.Keys.Intersect(attributeDataImmutable.Keys);
                        if (intersections.Any())
                        {
                            // TODO output diagnostic about duplicate entry.

                            continue;
                        }

                        var fullTemplateData = attributeDataImmutable.AddRange(target);

                        // All regular templates are rendered and combined.
                        foreach (var t in template.Templates)
                        {
                            sb.AppendLine(Mustache.RenderTemplate(t, fullTemplateData));
                            sb.AppendLine();
                        }

                        output.Add(
                            new MemberTargetAndTemplateData(
                                TemplateType.Complete,
                                ctypeName,
                                new ClassIdentifier(fullTemplateData[ClassNamespace].RenderString, fullTemplateData[ClassName].RenderString),
                                sb.ToString()));

                        foreach (var t in template.GlobalChildTemplates)
                        {
                            var childFragement = Mustache.RenderTemplate(t.Template, fullTemplateData);
                            output.Add(
                                new MemberTargetAndTemplateData(
                                    TemplateType.GlobalChild,
                                    t.GlobalTemplateName,
                                    new ClassIdentifier(fullTemplateData[ClassNamespace].RenderString, fullTemplateData[ClassName].RenderString),
                                    childFragement));
                        }

                        foreach (var t in template.FileTemplates)
                        {
                            var fileContents = Mustache.RenderTemplate(t.Template, fullTemplateData);
                            output.Add(
                                new MemberTargetAndTemplateData(
                                    TemplateType.File,
                                    ctypeName + "_" + t.FileName,
                                    new ClassIdentifier(fullTemplateData[ClassNamespace].RenderString, fullTemplateData[ClassName].RenderString),
                                    fileContents));
                        }
                    }
                }
            }
        }

        var builder = ImmutableArray.CreateBuilder<MemberTargetAndTemplateData>();
        builder.AddRange(output);
        return new DataOrDiagnostics<ImmutableArray<MemberTargetAndTemplateData>>(builder.ToImmutable(), diagnostics);
    }

    private static ImmutableList<ImmutableDictionary<string, Mustache.RenderData>> GetSyntaxData(SemanticModel semanticModel, MemberDeclarationSyntax mds)
    {
        var listOfData = ImmutableList.CreateBuilder<ImmutableDictionary<string, Mustache.RenderData>>();
        var data = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();

        if (mds is ClassDeclarationSyntax cds)
        {
            GetClassData(cds, semanticModel, data);
            listOfData.Add(data.ToImmutable());
        }
        else if (mds is FieldDeclarationSyntax fds)
        {
            var fieldType = fds.Declaration.Type.ToFullString();
            data.Add(new Mustache.RenderData("FieldType", fieldType, true));

            if (SyntaxHelpers.FindClass(fds) is ClassDeclarationSyntax fieldClass)
            {
                GetClassData(fieldClass, semanticModel, data);
            }

            if (fds.Declaration.Type is GenericNameSyntax gns)
            {
                var genericStart = gns.TypeArgumentList.LessThanToken.FullSpan.End;
                var genericEnd = gns.TypeArgumentList.GreaterThanToken.FullSpan.Start;
                var fs = fds.SyntaxTree.GetText().ToString(new TextSpan(genericStart, genericEnd - genericStart));

                data.Add(new Mustache.RenderData("FieldGenericType", fs, true));
            }
            else
            {
                data.Add(new Mustache.RenderData("FieldGenericType", string.Empty, false));
            }

            if (fds.Declaration.Variables.Count == 0)
            {
                // The field declaration isn't complete, just keep going.
                data.Add(new Mustache.RenderData("FieldName", string.Empty, false));
                listOfData.Add(data.ToImmutable());
            }
            else
            {
                var constData = data.ToImmutable();
                foreach (var variable in fds.Declaration.Variables)
                {
                    var varData = constData.ToBuilder();
                    varData.Add(new Mustache.RenderData("FieldName", variable.Identifier.Text, true));
                    listOfData.Add(varData.ToImmutable());
                }
            }
        }

        return listOfData.ToImmutable();

        static void GetClassData(ClassDeclarationSyntax cds, SemanticModel semanticModel, ImmutableDictionary<string, Mustache.RenderData>.Builder data)
        {
            var typeInfo = semanticModel.GetDeclaredSymbol(cds);

            if (typeInfo == null)
            {
                // TODO throw here?
                return;
            }

            data.Add(new Mustache.RenderData(ClassNamespace, typeInfo.ContainingNamespace.ToString(), true));
            data.Add(new Mustache.RenderData(ClassName, typeInfo.Name, true));
        }
    }

    /// <summary>
    /// Looks for an attribute that matches the name of one we created. It does this without relying
    /// on the compliation at all.
    /// </summary>
    /// <typeparam name="T">Any MemberDeclarationSyntax</typeparam>
    /// <param name="mds">Syntax we want to check for matching attributes</param>
    /// <param name="userGenerators">Complete list of attributes defined by the user</param>
    /// <returns>Syntax and template data, or null</returns>
    private static (T, ImmutableArray<UserGeneratorTemplateData>)? FindProbableTargets<T>(
        T mds,
        ImmutableArray<UserGeneratorTemplateData> userGenerators)
             where T : MemberDeclarationSyntax
    {
        foreach (var item in mds.AttributeLists.SelectMany(x => x.Attributes))
        {
            var classAttName = (item.Name as IdentifierNameSyntax)?.Identifier.Text;
            if (classAttName is null) continue;
            classAttName += classAttName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) ? "" : "Attribute";

            foreach (var gen in userGenerators)
            {
                var genAttName = gen.AttributeData.AttributeIdentifier.ClassName;
                if (genAttName == classAttName)
                {
                    return (mds, userGenerators);
                }
            }
        }

        return null;
    }

    private static bool IsTargetForGeneration(SyntaxNode node)
    {
        return (node is ClassDeclarationSyntax c && c.AttributeLists.Count > 0) ||
               (node is FieldDeclarationSyntax f && f.AttributeLists.Count > 0);
    }
}
