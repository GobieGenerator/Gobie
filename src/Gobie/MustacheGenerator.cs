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

namespace Mustache
{
    [Generator]
    public class MustacheGenerator : ISourceGenerator
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
                ////Trace.WriteLine(Environment.NewLine + "New Attribute:");
                var attName = a.Name;
                ////Trace.WriteLine(attName.ToString()); // This gets us the attribute name as written (w or w/o Attribute at the end)
                var sm = compilation.GetSemanticModel(a.SyntaxTree);
                var typeInfo = sm.GetTypeInfo(a);

                ////Trace.WriteLine("Type containing namespace: " + typeInfo.Type.ContainingNamespace);
                ////Trace.WriteLine("Type name: " + typeInfo.Type.Name);
                ////Trace.WriteLine("Type metadata name: " + typeInfo.Type.MetadataName);
                ////Trace.WriteLine("Type base type: " + typeInfo.Type.BaseType);
                ////Trace.WriteLine("Type base type name: " + typeInfo.Type.BaseType.Name);

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
                    if (FindField(a) is FieldDeclarationSyntax field)
                    {
                        ////var names = field.DescendantTokens(x => true);
                        ////foreach (var name in names)
                        ////{
                        ////    Trace.WriteLine("Descendant token: " + name.ToString() + " " + name.Kind());
                        ////}
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

                    var stubble = new StubbleBuilder().Build();
                    var ht = stubble.Render(template, dict);
                    context.AddSource($"Gobie_Field_{fieldName}", SourceText.From(ht, Encoding.UTF8));
                }
            }

            // If we know the MetadataName then we can recursivly find the base to get back to ours.
            ////var baseSymbol = compilation.GetTypeByMetadataName("Gobie.GobieAssemblyGeneratorAttribute");
            ////if (baseSymbol != null)
            ////{
            ////    var t = baseSymbol.BaseType;
            ////    var bt = t?.BaseType;
            ////    //var baseType = t.BaseType;
            ////}
        }

        private static FieldDeclarationSyntax? FindField(SyntaxNode node)
        {
            if (node is FieldDeclarationSyntax field)
            {
                return field;
            }
            if (node is SyntaxNode)
            {
                return FindField(node.Parent);
            }
            return null;
        }
    }
}
