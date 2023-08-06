namespace Gobie.Workflows;

using Gobie.Models.Unions;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;

public static class CodeGeneration
{
    public static void Output(SourceProductionContext spc, ImmutableArray<CodeOutput> sources)
    {
        foreach (var source in sources)
        {
            spc.AddSource(Sanatize(source.HintName), source.Code);
        }

        static string Sanatize(string rawHintName)
        {
            // Simplified and adapted from the code that validates the hintname. Replaces any
            // illegal characters with an underscore.
            var sb = new StringBuilder();
            foreach (var c in rawHintName)
            {
                if (char.IsLetterOrDigit(c)
                    || c == '.'
                    || c == ','
                    || c == '-'
                    || c == '+'
                    || c == '`'
                    || c == '_'
                    || c == ' '
                    || c == '('
                    || c == ')'
                    || c == '['
                    || c == ']'
                    || c == '{'
                    || c == '}')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }
    }

    public static IncrementalValueProvider<DataOrDiagnostics<ImmutableArray<CodeOutput>>> CollectOutputs(
        IncrementalValueProvider<(ImmutableArray<MemberTargetAndTemplateData> Left,
                                  ImmutableArray<AssemblyTargetAndTemplateData> Right)> targets)
    {
        return targets.Select(static (s, ct) => OutputFiles(s.Left, s.Right, ct));
    }

    private static DataOrDiagnostics<ImmutableArray<CodeOutput>> OutputFiles(
        ImmutableArray<MemberTargetAndTemplateData> memberTemplates,
        ImmutableArray<AssemblyTargetAndTemplateData> assemblyTemplates,
        CancellationToken ct)
    {
        var diagnostics = new List<Diagnostic>();

        var codeOut = ImmutableArray.CreateBuilder<CodeOutput>();
        var globalTemplates = new Dictionary<string, StringBuilder>();

        var fileGroups = memberTemplates.GroupBy(x => (x.TemplateType, x.TargetClass, x.GeneratorName));

        foreach (var group in fileGroups)
        {
            ct.ThrowIfCancellationRequested();

            if (group.Key.TemplateType == TemplateType.Complete)
            {
                var sb = new StringBuilder();
                sb.AppendLine(@$"
                    namespace {group.Key.TargetClass.NamespaceName}
                    {{
                        public partial class {group.Key.TargetClass.ClassName}
                        {{");

                foreach (var templates in group)
                {
                    sb.AppendLine(templates.Code);
                }

                sb.AppendLine(@"
                        }
                    }");

                var fullCode = CSharpSyntaxTree.ParseText(sb.ToString()).GetRoot().NormalizeWhitespace().ToFullString();
                var hintName = $"{group.Key.TargetClass.ClassName}_{group.Key.GeneratorName}.g.cs";
                codeOut.Add(new CodeOutput(hintName, fullCode));
            }
            else if (group.Key.TemplateType == TemplateType.File)
            {
                var code = group.First().Code; // TODO there should never be more than one.

                var fullCode = CSharpSyntaxTree.ParseText(code).GetRoot().NormalizeWhitespace().ToFullString();
                var hintName = $"{group.Key.TargetClass.ClassName}_{group.Key.GeneratorName}.g.cs";
                codeOut.Add(new CodeOutput(hintName, fullCode));
            }
            else if (group.Key.TemplateType == TemplateType.GlobalChild)
            {
                foreach (var frag in group)
                {
                    if (globalTemplates.TryGetValue(frag.GeneratorName, out var sb) == false)
                    {
                        sb = new StringBuilder();
                        globalTemplates.Add(frag.GeneratorName, sb);
                    }

                    sb.AppendLine(frag.Code);
                }
            }
        }

        // This is the first place that all the assembly attributes are gathered together and
        // processed at once. All attributes are configured to be unique so if more than one
        // instance exists we can rely on the compiler to provide an error. All we need to do is
        // avoid generating code more than once.
        var unqiueAssemblyTempaltes = assemblyTemplates.GroupBy(x => x.GlobalGeneratorName).Select(x => x.First());
        foreach (var assemblyTemplate in unqiueAssemblyTempaltes)
        {
            ct.ThrowIfCancellationRequested();

            var hintName = $"{assemblyTemplate.GeneratorLabel}.g.cs";
            var renderData = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();

            var code = globalTemplates.TryGetValue(assemblyTemplate.GeneratorLabel, out var sb) ? sb.ToString() : string.Empty;
            renderData.Add("ChildContent", new Mustache.RenderData("ChildContent", code, code.Length > 0));
            var renderedTemplate = Mustache.RenderTemplate(assemblyTemplate.GlobalTemplate, renderData.ToImmutable());
            var fullCode = CSharpSyntaxTree.ParseText(renderedTemplate).GetRoot().NormalizeWhitespace().ToFullString();

            codeOut.Add(new CodeOutput(hintName, fullCode));
        }

        return new(codeOut.ToImmutable(), diagnostics);
    }
}
