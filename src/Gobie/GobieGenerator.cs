using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Stubble.Core.Builders;
using static Gobie.Helpers.SyntaxHelpers;

namespace Gobie
{
    [Generator]
    public class GobieGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Wire up incremental generator here.
        }

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
