using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mustache
{
    [Generator]
    public class MustacheGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            string attributeSource = @"
    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple=true)]
    internal sealed class MustacheAttribute: System.Attribute
    {
        public string Name { get; }
        public string Template { get; }
        public string Hash { get; }
        public MustacheAttribute(string name, string template, string hash)
            => (Name, Template, Hash) = (name, template, hash);
    }
";
            context.AddSource("Mustache_MainAttributes__", SourceText.From(attributeSource, Encoding.UTF8));

            Compilation compilation = context.Compilation;

            IEnumerable<(string, string, string)> options = GetMustacheOptions(compilation);
            IEnumerable<(string, string)> namesSources = SourceFilesFromMustachePaths(options);

            foreach ((string name, string source) in namesSources)
            {
                context.AddSource($"Mustache{name}", SourceText.From(source, Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required
        }

        private static IEnumerable<(string, string, string)> GetMustacheOptions(Compilation compilation)
        {
            // Get all Mustache attributes
            IEnumerable<SyntaxNode>? allNodes = compilation.SyntaxTrees.SelectMany(s => s.GetRoot().DescendantNodes());
            IEnumerable<AttributeSyntax> allAttributes = allNodes.Where((d) => d.IsKind(SyntaxKind.Attribute)).OfType<AttributeSyntax>();
            ImmutableArray<AttributeSyntax> attributes = allAttributes.Where(d => d.Name.ToString() == "MustacheAttribute")
                .ToImmutableArray();

            IEnumerable<SemanticModel> models = compilation.SyntaxTrees.Select(st => compilation.GetSemanticModel(st));
            foreach (AttributeSyntax att in attributes)
            {
                string mustacheName = "", template = "", hash = "";
                int index = 0;

                if (att.ArgumentList is null) throw new Exception("Can't be null here");

                SemanticModel m = compilation.GetSemanticModel(att.SyntaxTree);

                foreach (AttributeArgumentSyntax arg in att.ArgumentList.Arguments)
                {
                    ExpressionSyntax expr = arg.Expression;

                    TypeInfo t = m.GetTypeInfo(expr);
                    Optional<object?> v = m.GetConstantValue(expr);
                    if (index == 0)
                    {
                        mustacheName = v.ToString();
                    }
                    else if (index == 1)
                    {
                        template = v.ToString();
                    }
                    else
                    {
                        hash = v.ToString();
                    }
                    index += 1;
                }
                yield return (mustacheName, template, hash);
            }
        }

        private static string SourceFileFromMustachePath(string name, string template, string hash)
        {
            var ht = HandlebarsDotNet.Handlebars.Compile(template)(hash);

            return GenerateMustacheClass(name, ht);
        }

        private static IEnumerable<(string, string)> SourceFilesFromMustachePaths(IEnumerable<(string, string, string)> pathsData)
        {
            foreach ((string name, string template, string hash) in pathsData)
            {
                var m = string.Empty;
                try
                {
                    m = SourceFileFromMustachePath(name, template, hash);
                }
                catch (Exception e)
                {
                    m = e.Message;
                }
                var t = (name, m);
                yield return t;
            }
        }

        private static string GenerateMustacheClass(string className, string mustacheText)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($@"
namespace Mustache {{
    public static partial class Constants {{
        public const string {className} = @""{mustacheText.Replace("\"", "\"\"")}"";
    }}
}}
");
            return sb.ToString();
        }
    }
}
