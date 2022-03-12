namespace Gobie.Workflows
{
    using Gobie.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
                    transform: static (ctx, _) => GetUserTemplate(ctx))
                .Where(static x => x is not null)!;
        }

        public static IncrementalValuesProvider<DataOrDiagnostics<UserGeneratorTemplateData>> GetFullGenerators(
            IncrementalValuesProvider<(UserGeneratorAttributeData Left, Compilation Right)> compliationAndGeneratorDeclarations)
        {
            return compliationAndGeneratorDeclarations.Select(static (s, _) => GetFullTemplateDeclaration(s));
        }

        private static DataOrDiagnostics<UserGeneratorTemplateData> GetFullTemplateDeclaration((UserGeneratorAttributeData, Compilation) s)
        {
            var (data, compliation) = (s.Item1, s.Item2);
            var diagnostics = new List<Diagnostic>();

            ////GetTemplates(data.ClassDeclarationSyntax, classSymbol, genData);

            ////if (cds.ToFullString().Contains("GobieTemplate") == false)
            ////{
            ////    diagnostics.Add(Diagnostic.Create(Warnings.UserTemplateIsEmpty, classLocation));
            ////}

            return new DataOrDiagnostics<UserGeneratorTemplateData>(diagnostics);
        }

        private static bool IsClassDeclaration(SyntaxNode node) => node is ClassDeclarationSyntax;

        private static DataOrDiagnostics<UserGeneratorAttributeData>? GetUserTemplate(GeneratorSyntaxContext context)
        {
            var cds = (ClassDeclarationSyntax)context.Node;
            var classLocation = cds.Identifier.GetLocation();

            if (cds.BaseList is null)
            {
                return null;
            }

            // Because we control the list of base types they can use this should be a very good
            // though imperfect filter we can run on the syntax alone.
            var gobieBaseTypeName = cds.BaseList.Types.SingleOrDefault(x => GobieGenerator.GobieGeneratorBaseClasses.Contains(x.ToString()));
            if (gobieBaseTypeName is null)
            {
                return null;
            }

            //! We accumulate data here.
            var genData = new UserGeneratorAttributeData(cds.Identifier.ToString(), cds);

            var diagnostics = new List<Diagnostic>();
            if (cds.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            {
                diagnostics.Add(Diagnostic.Create(Errors.UserTemplateIsPartial, classLocation));
            }

            if (cds.Modifiers.Any(x => x.IsKind(SyntaxKind.SealedKeyword)) == false)
            {
                diagnostics.Add(Diagnostic.Create(Errors.UserTemplateIsNotSealed, classLocation));
            }

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(context.Node);

            var invalidName = !cds.Identifier.ToString().EndsWith("Generator", StringComparison.OrdinalIgnoreCase);
            foreach (var attribute in classSymbol!.GetAttributes())
            {
                var b = attribute?.AttributeClass?.ToString();
                if (attribute?.AttributeClass?.ToString() == "Gobie.GobieGeneratorNameAttribute")
                {
                    if (attribute!.ConstructorArguments.Count() == 0)
                    {
                        continue;
                    }

                    var genName = attribute!.ConstructorArguments[0].Value!.ToString();

                    string G(AttributeData d, int index)
                    {
                        if (d.ConstructorArguments.Length > index)
                        {
                        }

                        return string.Empty;
                    }

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
                diagnostics.Add(Diagnostic.Create(Errors.GeneratorNameInvalid, classLocation));
            }

            //! Diagnostics before here are errors that stop generation.
            if (diagnostics.Any())
            {
                return new(diagnostics);
            }

            var requiredPropertyNumber = 1;
            foreach (PropertyDeclarationSyntax node in cds.ChildNodes().Where(x => x is PropertyDeclarationSyntax))
            {
                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(node);
                if (propertySymbol is null)
                {
                    // TODO is this a problem?
                    continue;
                }

                foreach (var att in propertySymbol.GetAttributes())
                {
                    var b = att?.AttributeClass?.ToString();
                    if (b == "Gobie.Required")
                    {
                        genData.RequiredParameters.Add(
                            new RequiredParameter(
                                1,
                                requiredPropertyNumber,
                                "ReqParam",
                                "string"));

                        requiredPropertyNumber++;
                        goto RequiredPropertyHandeled;
                    }
                }

                genData.OptionalParameters.Add(node.ToString());
            RequiredPropertyHandeled:;
            }

            return new(genData);
        }

        private static void BuildUserGeneratorAttributes(SourceProductionContext spc, UserGeneratorAttributeData data)
        {
            var generatedCode = @$"

            namespace {data.NamespaceName}
            {{
                /// <summary> This attribute will cause the generator defined by this thing here to
                /// run <see cref=""TODONAMESPACE.{data.DefinitionIdentifier}""/> to run. </summary>
                public sealed class {data.AttributeIdentifier} : Gobie.GobieFieldGeneratorAttribute
                {{
                    public {data.AttributeIdentifier}()
                    {{
                    }}

                    {string.Join(Environment.NewLine, data.OptionalParameters)}
                }}
            }}
            ";

            generatedCode = CSharpSyntaxTree.ParseText(generatedCode).GetRoot().NormalizeWhitespace().ToFullString();
            spc.AddSource($"{data.AttributeIdentifier}.g.cs", generatedCode);
        }
    }
}
