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
    public class GobieGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            Compilation compilation = context.Compilation;

            RunGobie(compilation, context);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required
        }

        private static void RunGobie(Compilation compilation, GeneratorExecutionContext context)
        {
            // Get all Mustache attributes
            IEnumerable<SyntaxNode>? allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            var attributeTemplates = new Dictionary<string, List<string>>();
            GetCustomUserTemplateDefinitions(compilation, context, allNodes, attributeTemplates);
            ProcessAttributes(compilation, context, allNodes, attributeTemplates);
        }

        private static void GetCustomUserTemplateDefinitions(Compilation compilation, GeneratorExecutionContext context, IEnumerable<SyntaxNode> allNodes, Dictionary<string, List<string>> attributeTemplates)
        {
            IEnumerable<ClassDeclarationSyntax> allClasses = allNodes.Where((d) => d.IsKind(SyntaxKind.ClassDeclaration)).OfType<ClassDeclarationSyntax>();
            foreach (var c in allClasses)
            {
                // Check if its inherited from one of our attributes.
                if (!ClassInheritsFrom(compilation, c, "GobieAssemblyGeneratorBaseAttribute"))
                {
                    continue;
                }

                if (c.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GobieAttributeIsPartial, c.GetLocation()));
                    continue;
                }
                else if (c.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
                {
                    continue; // We don't care about abstract instances.
                }

                var attTemplates = new List<string>();
                IEnumerable<AttributeSyntax> allAttributes = c.DescendantNodesAndSelf((_) => true, false).Where((d) => d.IsKind(SyntaxKind.Attribute)).OfType<AttributeSyntax>();
                foreach (var attribute in allAttributes)
                {
                    var sm = compilation.GetSemanticModel(attribute.SyntaxTree);
                    var typeInfo = sm.GetTypeInfo(attribute);
                    //If it is, pull all the template info off of this instance
                    if (typeInfo.Type?.Name == "GobieTemplateAttribute" || typeInfo.Type?.BaseType?.Name == "GobieTemplateAttribute")
                    {
                        var template = GetTemplateOrIssueDiagnostic(compilation, context, attribute);
                        if (template != null)
                        {
                            attTemplates.Add(template);
                        }
                    }
                }

                if (attTemplates.Any())
                {
                    attributeTemplates.Add(GetClassname(compilation, c), attTemplates);
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.GobieAttributeHasNoTemplates, c.GetLocation()));
                }
            }
        }

        private static void ProcessAttributes(Compilation compilation, GeneratorExecutionContext context, IEnumerable<SyntaxNode> allNodes, Dictionary<string, List<string>> attributeTemplates)
        {
            IEnumerable<AttributeSyntax> allAttributes = allNodes.Where((d) => d.IsKind(SyntaxKind.Attribute)).OfType<AttributeSyntax>();
            foreach (var a in allAttributes)
            {
                var attName = a.Name;
                var sm = compilation.GetSemanticModel(a.SyntaxTree);
                var typeInfo = sm.GetTypeInfo(a);

                if (typeInfo.Type?.BaseType?.Name == "GobieFieldGeneratorAttribute")
                {
                    var customAttrTypeName = typeInfo.Type.Name;
                    var fieldName = string.Empty;
                    var dict = new Dictionary<string, string>();

                    if (!attributeTemplates.TryGetValue(customAttrTypeName, out var templates))
                    {
                        // todo issue a diagnostic?
                        continue;
                    }

                    Trace.WriteLine("Found a gobie generator:");

                    if (FindClass(a) is ClassDeclarationSyntax classDeclaration)
                    {
                        if (classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
                        {
                            // Continue
                        }
                        else
                        {
                            context.ReportDiagnostic(Diagnostic.Create(Diagnostics.ClassIsNotParital, classDeclaration.GetLocation()));
                            continue;
                        }
                    }
                    else
                    {
                        // TODO return; // How wouldn't this be in the class.
                    }

                    if (FindField(a) is FieldDeclarationSyntax field)
                    {
                        SemanticModel model = compilation.GetSemanticModel(field.SyntaxTree);
                        foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
                        {
                            Trace.WriteLine("variable " + variable);

                            // Get the symbol being decleared by the field, and keep it if its annotated
                            IFieldSymbol fieldSymbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;

                            Trace.WriteLine("Annotated Field is: '" + fieldSymbol?.Name + "'");
                            fieldName = fieldSymbol?.Name;
                            if (fieldName?.Length > 0)
                            {
                                dict.Add("field", fieldName);
                                dict.Add("Property", CultureInfo.InvariantCulture.TextInfo.ToTitleCase(fieldName));
                            }
                        }
                    }

                    var sb = new StringBuilder();
                    foreach (var template in templates)
                    {
                        sb.AppendLine(RenderTemplate(dict, template, true));
                        sb.AppendLine();
                    }

                    context.AddSource($"Gobie_Field_{fieldName}", SourceText.From(sb.ToString(), Encoding.UTF8));
                }
            }
        }

        private static string? GetTemplateOrIssueDiagnostic(Compilation compilation, GeneratorExecutionContext context, AttributeSyntax a)
        {
            if (FindField(a) is FieldDeclarationSyntax field)
            {
                if (!field.Modifiers.Any(x => x.IsKind(SyntaxKind.ConstKeyword)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.TemplateIsNotConstString, field.GetLocation()));
                }

                //We assume its only one variable.
                var init = field.Declaration.Variables[0];
                if (init.Initializer?.Value is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    var sm = compilation.GetSemanticModel(a.SyntaxTree);
                    return sm.GetConstantValue(literal).ToString();
                }
                else
                {
                    context.ReportDiagnostic(Diagnostic.Create(Diagnostics.TemplateIsNotConstString, field.GetLocation()));
                }
            }

            return null;
        }

        private static string RenderTemplate(Dictionary<string, string> dict, string template, bool debug)
        {
            // TODO only debug when requested.

            var sb = new StringBuilder();
            var stubble = new StubbleBuilder().Build();
            var ht = stubble.Render(template, dict);
            var len = dict.Max(x => x.Key.Length) + 1;

            if (debug)
            {
                sb.AppendLine($"// Gobie Debug");
                sb.AppendLine($"// ---------------------------------------");
                sb.AppendLine($"// Dictionary:");
                foreach (var item in dict.OrderBy(x => x.Key))
                {
                    sb.AppendLine($"// {item.Key.PadRight(len)}: '{item.Value}'");
                }
                sb.AppendLine();
                sb.AppendLine($"// Source Template:");
                foreach (var templateLine in template.Split('\n'))
                {
                    sb.Append($"// {templateLine}{(templateLine.EndsWith("\r") ? "\n" : string.Empty)}");
                }
                sb.AppendLine();
            }

            sb.AppendLine(ht);
            return sb.ToString();
        }
    }
}
