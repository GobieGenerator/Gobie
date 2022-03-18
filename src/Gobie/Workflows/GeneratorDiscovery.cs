namespace Gobie.Workflows
{
    using Gobie.Diagnostics;
    using Gobie.Helpers;
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

            var model = compliation.GetSemanticModel(data.ClassDeclarationSyntax.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(data.ClassDeclarationSyntax);

            if (symbol is null)
            {
                return new(diagnostics);
            }

            var templates = GetTemplates(data.ClassDeclarationSyntax, symbol, data, compliation);

            var td = new UserGeneratorTemplateData(data, templates);

            return new(td, diagnostics);
        }

        private static List<string> GetTemplates(ClassDeclarationSyntax cds, ISymbol classSymbol, UserGeneratorAttributeData genData, Compilation compliation)
        {
            var templates = new List<string>();

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

                                if (fieldSymbol is IFieldSymbol fs && fs.ConstantValue is not null)
                                {
                                    templates.Add(fs.ConstantValue.ToString());
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
                            Warnings.PriorityAlreadyDeclared(req.RequestedOrder),
                            req.RequestedOrderLocation));
                }
            }

            return diagnostics;
        }

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
            var ident = new ClassIdentifier("Gobie", cds.Identifier.ToString());
            var genData = new UserGeneratorAttributeData(ident, cds);

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
                if (ConstantTypes.IsAllowedConstantType(node.Type, out var propertyType) == false)
                {
                    // We don't need to break the whole template when they do this wrong.
                    diagnostics.Add(Diagnostic.Create(Errors.DisallowedTemplateParameterType("TODO"), node.Type.GetLocation()));
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
                        // Get the requested order. If we can't get it presumably the compiler is
                        // complaining to the user about providing an invalid value.
                        var orderArg = att.ConstructorArguments[0].Value;
                        var order = orderArg is int i ? i : int.MaxValue;


                        genData.AddRequiredParameter(
                            new RequiredParameter(
                                order,
                                node.GetLocation(),
                                requiredPropertyNumber,
                                node.Identifier.Text,
                                propertyType));

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
                public sealed class {data.AttributeIdentifier.ClassName} : Gobie.GobieFieldGeneratorAttribute
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
}
