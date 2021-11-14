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

            GetMustacheOptions(compilation, context);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required
        }

        private static void GetMustacheOptions(Compilation compilation, GeneratorExecutionContext context)
        {
            // Get all Mustache attributes
            IEnumerable<SyntaxNode>? allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            IEnumerable<AttributeSyntax> allAttributes = allNodes.Where((d) => d.IsKind(SyntaxKind.Attribute)).OfType<AttributeSyntax>();

            foreach (var a in allAttributes)
            {
                var attName = a.Name;
                var sm = compilation.GetSemanticModel(a.SyntaxTree);
                var typeInfo = sm.GetTypeInfo(a);

                if (typeInfo.Type?.Name == "GobieFieldGeneratorAttribute" || typeInfo.Type?.BaseType?.Name == "GobieFieldGeneratorAttribute")
                {
                    var fieldName = string.Empty;
                    var dict = new Dictionary<string, string>();
                    var template = string.Empty;
                    SemanticModel m = compilation.GetSemanticModel(a.SyntaxTree);
                    var index = 0;
                    foreach (AttributeArgumentSyntax arg in a.ArgumentList.Arguments)
                    {
                        ExpressionSyntax expr = arg.Expression;

                        TypeInfo t = m.GetTypeInfo(expr);
                        Optional<object?> v = m.GetConstantValue(expr);

                        if (index == 0)
                        {
                            template = v.ToString();
                        }
                        index++;
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
                            // TODO return;
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

                    var genCode = RenderTemplate(dict, template, true);

                    context.AddSource($"Gobie_Field_{fieldName}", SourceText.From(genCode, Encoding.UTF8));
                }
            }
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
