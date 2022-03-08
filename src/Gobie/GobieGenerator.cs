using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Gobie.Diagnostics;

namespace Gobie
{
    [Generator]
    public class GobieGenerator : IIncrementalGenerator
    {
        private static readonly HashSet<string> GobieBase = new()
        {
            "GobieFieldGenerator",
            "Gobie.GobieFieldGenerator",
        };

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "EnumExtensionsAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.GobieCore, Encoding.UTF8)));

            //### Find the user templates and report diagnostics on issue.

            IncrementalValuesProvider<DataOrDiagnostics<UserGeneratorData>> userTemplateSyntaxOrDiagnostics =
                context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: static (s, _) => IsClassDeclaration(s),
                        transform: static (ctx, _) => GetUserTemplate(ctx))
                    .Where(static x => x is not null)!;

            context.RegisterSourceOutput(
                userTemplateSyntaxOrDiagnostics,
                static (spc, source) => OutputDiagnostics(spc, source));

            IncrementalValuesProvider<UserGeneratorData> userTemplateSyntax =
                userTemplateSyntaxOrDiagnostics
                    .Where(x => x.Data is not null)
                    .Select(selector: (s, _) => s.Data!);

            context.RegisterSourceOutput(
                userTemplateSyntax,
                static (spc, source) => BuildUserGeneratorAttributes(spc, source));

            //### Generate attributes for the defined templates.

            //### Find usage of the user's attributes and generate their code.

            //////### Andrew lock's example
            ////IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations =
            ////    context.SyntaxProvider
            ////        .CreateSyntaxProvider(
            ////            predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select enums with attributes
            ////            transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
            ////        .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

            ////// Combine the selected enums with the `Compilation`
            ////IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums =
            ////    context.CompilationProvider.Combine(enumDeclarations.Collect());

            ////// Generate the source using the compilation and enums
            ////context.RegisterSourceOutput(
            ////    compilationAndEnums,
            ////    static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static void BuildUserGeneratorAttributes(SourceProductionContext spc, UserGeneratorData source)
        {
            var generatedCode = @$"

            namespace {source.NamespaceName}
            {{
                /// <summary> This attribute will cause the generator defined by this thing here to
                /// run <see cref=""TODONAMESPACE.{source.DefinitionIdentifier}""/> to run. </summary>
                public sealed class {source.AttributeIdentifier} : Gobie.GobieFieldGeneratorAttribute
                {{
                }}
            }}
            ";

            generatedCode = CSharpSyntaxTree.ParseText(generatedCode).GetRoot().NormalizeWhitespace().ToFullString();
            spc.AddSource($"{source.AttributeIdentifier}.g.cs", generatedCode);
        }

        private static void OutputDiagnostics<T>(SourceProductionContext spc, DataOrDiagnostics<T> option)
        {
            if (option.Diagnostics is not null)
                foreach (var diagnostic in option.Diagnostics)
                    spc.ReportDiagnostic(diagnostic);
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node) =>
            node is EnumDeclarationSyntax m && m.AttributeLists.Count > 0;

        private static bool IsClassDeclaration(SyntaxNode node) =>
            node is ClassDeclarationSyntax;

        private static DataOrDiagnostics<UserGeneratorData>? GetUserTemplate(GeneratorSyntaxContext context)
        {
            var cds = (ClassDeclarationSyntax)context.Node;
            var classLocation = cds.Identifier.GetLocation();

            if (cds.BaseList is null)
            {
                return null;
            }

            // Because we control the list of base types they can use this should be a very good
            // though imperfect filter we can run on the syntax alone.
            var gobieBaseTypeName = cds.BaseList.Types.SingleOrDefault(x => GobieBase.Contains(x.ToString()));
            if (gobieBaseTypeName is null)
            {
                return null;
            }

            //! We accumulate data here.
            var genData = new UserGeneratorData(cds.Identifier.ToString());

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

            if (cds.ToFullString().Contains("GobieTemplate") == false)
            {
                diagnostics.Add(Diagnostic.Create(Warnings.UserTemplateIsEmpty, classLocation));
            }

            return new(genData, diagnostics);
        }

        private static EnumDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
            var enumDeclarationSyntax = (EnumDeclarationSyntax)context.Node;

            // loop through all the attributes on the method
            foreach (AttributeListSyntax attributeListSyntax in enumDeclarationSyntax.AttributeLists)
            {
                foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    {
                        // weird, we couldn't get the symbol, ignore it
                        continue;
                    }

                    INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                    string fullName = attributeContainingTypeSymbol.ToDisplayString();

                    // Is the attribute the [EnumExtensions] attribute?
                    if (fullName == "NetEscapades.EnumGenerators.EnumExtensionsAttribute")
                    {
                        // return the enum
                        return enumDeclarationSyntax;
                    }
                }
            }

            // we didn't find the attribute we were looking for
            return null;
        }

        ////private static void Execute(Compilation compilation, ImmutableArray<EnumDeclarationSyntax> enums, SourceProductionContext context)
        ////{
        ////    if (enums.IsDefaultOrEmpty)
        ////    {
        ////        // nothing to do yet
        ////        return;
        ////    }

        ////    // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems
        ////    // like a good idea!
        ////    IEnumerable<EnumDeclarationSyntax> distinctEnums = enums.Distinct();

        ////    // Convert each EnumDeclarationSyntax to an EnumToGenerate
        ////    List<EnumToGenerate> enumsToGenerate = GetTypesToGenerate(compilation, distinctEnums, context.CancellationToken);

        ////    // If there were errors in the EnumDeclarationSyntax, we won't create an EnumToGenerate
        ////    // for it, so make sure we have something to generate
        ////    if (enumsToGenerate.Count > 0)
        ////    {
        ////        // generate the source code and add it to the output
        ////        string result = GenerateExtensionClass(enumsToGenerate);
        ////        context.AddSource("EnumExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
        ////    }
        ////}

        ////private static List<EnumToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<EnumDeclarationSyntax> enums, CancellationToken ct)
        ////{
        ////    // Create a list to hold our output
        ////    var enumsToGenerate = new List<EnumToGenerate>();
        ////    // Get the semantic representation of our marker attribute
        ////    INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName("NetEscapades.EnumGenerators.EnumExtensionsAttribute");

        ////    if (enumAttribute == null)
        ////    {
        ////        // If this is null, the compilation couldn't find the marker attribute type which
        ////        // suggests there's something very wrong! Bail out..
        ////        return enumsToGenerate;
        ////    }

        ////    foreach (EnumDeclarationSyntax enumDeclarationSyntax in enums)
        ////    {
        ////        // stop if we're asked to
        ////        ct.ThrowIfCancellationRequested();

        ////        // Get the semantic representation of the enum syntax
        ////        SemanticModel semanticModel = compilation.GetSemanticModel(enumDeclarationSyntax.SyntaxTree);
        ////        if (semanticModel.GetDeclaredSymbol(enumDeclarationSyntax) is not INamedTypeSymbol enumSymbol)
        ////        {
        ////            // something went wrong, bail out
        ////            continue;
        ////        }

        ////        // Get the full type name of the enum e.g. Colour, or OuterClass<T>.Colour if it was
        ////        // nested in a generic type (for example)
        ////        string enumName = enumSymbol.ToString();

        ////        // Get all the members in the enum
        ////        ImmutableArray<ISymbol> enumMembers = enumSymbol.GetMembers();
        ////        var members = new List<string>(enumMembers.Length);

        ////        // Get all the fields from the enum, and add their name to the list
        ////        foreach (ISymbol member in enumMembers)
        ////        {
        ////            if (member is IFieldSymbol field && field.ConstantValue is not null)
        ////            {
        ////                members.Add(member.Name);
        ////            }
        ////        }

        ////        // Create an EnumToGenerate for use in the generation phase
        ////        enumsToGenerate.Add(new EnumToGenerate(enumName, members));
        ////    }

        ////    return enumsToGenerate;
        ////}

        ////        public void Execute(GeneratorExecutionContext context)
        ////        {
        ////            Compilation compilation = context.Compilation;

        ////            try
        ////            {
        ////                RunGobie(compilation, context);
        ////            }
        ////            catch (Exception ex)
        ////            {
        ////                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GobieCrashed(ex.Message + ": " + ex.StackTrace), null));
        ////            }
        ////        }

        ////        private static void RunGobie(Compilation compilation, GeneratorExecutionContext context)
        ////        {
        ////            // Get all Mustache attributes
        ////            IEnumerable<SyntaxNode>? allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
        ////            var attributeTemplates = new Dictionary<string, List<string>>();
        ////            var partialClassContents = new Dictionary<(string namespaceName, string className), string>();
        ////            var fileContents = new Dictionary<string, string>();

        ////            GetCustomUserTemplateDefinitions(compilation, context, allNodes, attributeTemplates);
        ////            ProcessAttributes(compilation, context, allNodes, attributeTemplates, partialClassContents);
        ////            OutputPartialClasses(context, partialClassContents);
        ////        }

        ////        private static void OutputPartialClasses(GeneratorExecutionContext context, Dictionary<(string namespaceName, string className), string> partialClassContents)
        ////        {
        ////            foreach (var pc in partialClassContents)
        ////            {
        ////                string generatedCode = BuildPartialClass(pc.Key.namespaceName, pc.Key.className, pc.Value);
        ////                generatedCode = CSharpSyntaxTree.ParseText(generatedCode).GetRoot().NormalizeWhitespace().ToFullString();
        ////                context.AddSource($"{pc.Key.namespaceName}.{pc.Key.className}.g", SourceText.From(generatedCode, Encoding.UTF8));
        ////            }
        ////        }

        ////        private static void GetCustomUserTemplateDefinitions(Compilation compilation, GeneratorExecutionContext context, IEnumerable<SyntaxNode> allNodes, Dictionary<string, List<string>> attributeTemplates)
        ////        {
        ////            IEnumerable<ClassDeclarationSyntax> allClasses = allNodes.Where((d) => d.IsKind(SyntaxKind.ClassDeclaration)).OfType<ClassDeclarationSyntax>();
        ////            foreach (var c in allClasses)
        ////            {
        ////                // Check if its inherited from one of our attributes.
        ////                if (!ClassInheritsFrom(compilation, c, "GobieGeneratorBase"))
        ////                {
        ////                    continue;
        ////                }

        ////                if (c.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
        ////                {
        ////                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GobieAttributeIsPartial, c.GetLocation()));
        ////                    continue;
        ////                }
        ////                else if (c.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
        ////                {
        ////                    continue; // We don't care about abstract instances.
        ////                }

        ////                var attTemplates = new List<string>();
        ////                IEnumerable<AttributeSyntax> allAttributes =
        ////                    c.DescendantNodesAndSelf((_) => true, false)
        ////                     .Where((d) => d.IsKind(SyntaxKind.Attribute))
        ////                     .OfType<AttributeSyntax>();

        ////                foreach (var attribute in allAttributes)
        ////                {
        ////                    var sm = compilation.GetSemanticModel(attribute.SyntaxTree);
        ////                    var typeInfo = sm.GetTypeInfo(attribute);
        ////                    //If it is, pull all the template info off of this instance
        ////                    if (typeInfo.Type?.Name == "GobieBaseFieldAttribute" || typeInfo.Type?.BaseType?.Name == "GobieBaseFieldAttribute")
        ////                    {
        ////                        var template = GetTemplateOrIssueDiagnostic(compilation, context, attribute);
        ////                        if (template != null)
        ////                        {
        ////                            attTemplates.Add(template);
        ////                        }
        ////                    }
        ////                }

        ////                if (attTemplates.Any())
        ////                {
        ////                    var baseName = GetClassname(compilation, c);
        ////                    if (baseName.EndsWith("Generator"))
        ////                    {
        ////                        baseName = baseName.Substring(0, baseName.Length - 9);
        ////                    }
        ////                    baseName += "Attribute";
        ////                    // TODO output the actual attribute here.

        ////                    attributeTemplates.Add(baseName, attTemplates);
        ////                }
        ////                else
        ////                {
        ////                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GobieAttributeHasNoTemplates, c.GetLocation()));
        ////                }
        ////            }

        ////            Trace.WriteLine($"Found these User Templates");
        ////            foreach (var at in attributeTemplates)
        ////            {
        ////                Trace.WriteLine($"{at.Key}");
        ////            }
        ////            Trace.WriteLine($"Found these User Templates");
        ////        }

        ////        private static void ProcessAttributes(
        ////            Compilation compilation,
        ////            GeneratorExecutionContext context,
        ////            IEnumerable<SyntaxNode> allNodes,
        ////            Dictionary<string, List<string>> attributeTemplates,
        ////            Dictionary<(string namespaceName, string className), string> partialClassContents)
        ////        {
        ////            IEnumerable<AttributeSyntax> allAttributes = allNodes.Where((d) => d.IsKind(SyntaxKind.Attribute)).OfType<AttributeSyntax>();
        ////            foreach (var a in allAttributes)
        ////            {
        ////                var attName = a.Name;
        ////                var sm = compilation.GetSemanticModel(a.SyntaxTree);
        ////                var typeInfo = sm.GetTypeInfo(a);

        ////                if (typeInfo.Type?.BaseType?.Name == "GobieFieldGeneratorAttribute")
        ////                {
        ////                    var customAttrTypeName = typeInfo.Type.Name;
        ////                    var fieldName = string.Empty;
        ////                    var dict = new Dictionary<string, string>();
        ////                    INamedTypeSymbol? attributeSymbol = compilation.GetTypeByMetadataName("Gobie." + customAttrTypeName);
        ////                    if (attributeSymbol is null)
        ////                    {
        ////                        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GobieUnknownError("Couldn't find attribute"), a.GetLocation()));
        ////                        continue;
        ////                    }

        ////                    if (!attributeTemplates.TryGetValue(customAttrTypeName, out var templates))
        ////                    {
        ////                        context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GobieUnknownError("Couldn't find attribute templates"), a.GetLocation()));
        ////                        continue;
        ////                    }

        ////                    Trace.WriteLine("Found a gobie generator:");

        ////                    if (FindClass(a) is ClassDeclarationSyntax classDeclaration)
        ////                    {
        ////                        if (classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
        ////                        {
        ////                            // Continue
        ////                        }
        ////                        else
        ////                        {
        ////                            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.ClassIsNotParital, classDeclaration.GetLocation()));
        ////                            continue;
        ////                        }
        ////                    }
        ////                    else
        ////                    {
        ////                        continue;
        ////                        // TODO return; // How wouldn't this be in the class.
        ////                    }

        ////                    var templateDebug = false;
        ////                    INamedTypeSymbol? partialClass = null;
        ////                    if (FindField(a) is FieldDeclarationSyntax field)
        ////                    {
        ////                        SemanticModel model = compilation.GetSemanticModel(field.SyntaxTree);
        ////                        foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
        ////                        {
        ////                            Trace.WriteLine("variable " + variable);

        ////                            // Get the symbol being decleared by the field, and keep it if its annotated
        ////                            IFieldSymbol? fieldSymbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;
        ////                            if (fieldSymbol is null)
        ////                            {
        ////                                context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GobieUnknownError("Couldn't find field symbol"), a.GetLocation()));
        ////                                continue;
        ////                            }

        ////                            partialClass = fieldSymbol.ContainingType;

        ////                            Trace.WriteLine("Annotated Field is: '" + fieldSymbol?.Name + "'");
        ////                            var attributeData = fieldSymbol.GetAttributes().Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

        ////                            fieldName = fieldSymbol?.Name;
        ////                            if (fieldName?.Length > 0)
        ////                            {
        ////                                dict.Add("field", fieldName);
        ////                                dict.Add("Property", CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fieldName));

        ////                                foreach (var na in attributeData.NamedArguments)
        ////                                {
        ////                                    Trace.WriteLine($"NamedArgument {na.Key}='{na.Value.Value.ToString()}'");
        ////                                    if (na.Key == "TemplateDebug")
        ////                                    {
        ////                                        templateDebug = bool.Parse(na.Value.Value.ToString());
        ////                                    }
        ////                                    else
        ////                                    {
        ////                                        dict.Add(na.Key, na.Value.Value.ToString());
        ////                                    }
        ////                                }
        ////                            }
        ////                        }
        ////                    }

        ////                    if (partialClass != null)
        ////                    {
        ////                        var sb = new StringBuilder();
        ////                        foreach (var template in templates)
        ////                        {
        ////                            sb.AppendLine(RenderTemplate(dict, template, templateDebug));
        ////                            sb.AppendLine();
        ////                        }

        ////                        var key = (GetNamespaces(partialClass), partialClass.Name);
        ////                        if (partialClassContents.TryGetValue(key, out var contents))
        ////                        {
        ////                            partialClassContents[key] = contents + Environment.NewLine + sb.ToString();
        ////                        }
        ////                        else
        ////                        {
        ////                            partialClassContents.Add(key, sb.ToString());
        ////                        }
        ////                    }
        ////                }
        ////            }
        ////        }

        ////        private static string BuildPartialClass(string fullNamespace, string typeName, string v)
        ////        {
        ////            return
        ////        @$"using System;
        ////using System.Collections.Generic;
        ////using System.Linq;
        ////using System.Text;
        ////using System.Threading.Tasks;

        ////namespace {fullNamespace}
        ////{{
        ////    public partial class {typeName}
        ////    {{
        ////{v}
        ////    }}
        ////}}";
        ////        }

        ////        private static string GetNamespaces(ITypeSymbol type)
        ////        {
        ////            var n = type.ContainingNamespace;
        ////            var ns = n.Name;
        ////            n = n.ContainingNamespace;
        ////            while (n is INamespaceSymbol && !n.IsGlobalNamespace)
        ////            {
        ////                ns = n.Name + "." + ns;
        ////                n = n.ContainingNamespace;
        ////            }
        ////            return ns;
        ////        }

        ////        private static string? GetTemplateOrIssueDiagnostic(Compilation compilation, GeneratorExecutionContext context, AttributeSyntax a)
        ////        {
        ////            if (FindField(a) is FieldDeclarationSyntax field)
        ////            {
        ////                if (!field.Modifiers.Any(x => x.IsKind(SyntaxKind.ConstKeyword)))
        ////                {
        ////                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.TemplateIsNotConstString, field.GetLocation()));
        ////                }

        ////                //We assume its only one variable.
        ////                var init = field.Declaration.Variables[0];
        ////                if (init.Initializer?.Value is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
        ////                {
        ////                    var sm = compilation.GetSemanticModel(a.SyntaxTree);
        ////                    return sm.GetConstantValue(literal).ToString();
        ////                }
        ////                else
        ////                {
        ////                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.TemplateIsNotConstString, field.GetLocation()));
        ////                }
        ////            }

        ////            return null;
        ////        }

        ////        private static string RenderTemplate(Dictionary<string, string> dict, string template, bool debug)
        ////        {
        ////            var sb = new StringBuilder();
        ////            var stubble = new StubbleBuilder().Build();
        ////            var ht = stubble.Render(template, dict);
        ////            var len = dict.Max(x => x.Key.Length) + 1;

        ////            if (debug)
        ////            {
        ////                sb.AppendLine($"// Gobie Debug");
        ////                sb.AppendLine($"// ---------------------------------------");
        ////                sb.AppendLine($"// Dictionary:");
        ////                foreach (var item in dict.OrderBy(x => x.Key))
        ////                {
        ////                    sb.AppendLine($"// {item.Key.PadRight(len)}: '{item.Value}'");
        ////                }
        ////                sb.AppendLine();
        ////                sb.AppendLine($"// Source Template:");
        ////                foreach (var templateLine in template.Split('\n'))
        ////                {
        ////                    sb.Append($"// {templateLine}{(templateLine.EndsWith("\r") ? "\n" : string.Empty)}");
        ////                }
        ////                sb.AppendLine();
        ////            }

        ////            sb.AppendLine(ht);
        ////            return sb.ToString();
        ////        }
    }
}
