namespace Gobie.Workflows;

using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

public static class CodeGeneration
{
    public static void Output(SourceProductionContext spc, ImmutableArray<CodeOutput> sources)
    {
        foreach (var source in sources)
        {
            spc.AddSource(source.HintName, source.Code);
        }
    }

    public static IncrementalValueProvider<ImmutableArray<CodeOutput>> CollectOutputs(
        IncrementalValueProvider<(ImmutableArray<MemberTargetAndTemplateData> Left,
                                  ImmutableArray<AssemblyTargetAndTemplateData> Right)> targets)
    {
        return targets.Select(static (s, ct) => OutputFiles(s.Left, s.Right, ct));
    }

    private static ImmutableArray<CodeOutput> OutputFiles(
        ImmutableArray<MemberTargetAndTemplateData> memberTemplates,
        ImmutableArray<AssemblyTargetAndTemplateData> assemblyTemplates,
        CancellationToken ct)
    {
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

        foreach (var assemblyTemplate in assemblyTemplates)
        {
            ct.ThrowIfCancellationRequested();

            var hintName = $"{assemblyTemplate.GlobalGeneratorName}.g.cs";
            var renderData = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();

            var code = globalTemplates.TryGetValue(assemblyTemplate.GlobalGeneratorName, out var sb) ? sb.ToString() : string.Empty;
            renderData.Add("ChildContent", new Mustache.RenderData("ChildContent", code, code.Length > 0));
            var renderedTemplate = Mustache.RenderTemplate(assemblyTemplate.GlobalTemplate, renderData.ToImmutable());
            var fullCode = CSharpSyntaxTree.ParseText(renderedTemplate).GetRoot().NormalizeWhitespace().ToFullString();

            codeOut.Add(new CodeOutput(hintName, fullCode));
        }

        return codeOut.ToImmutable();
    }
}
