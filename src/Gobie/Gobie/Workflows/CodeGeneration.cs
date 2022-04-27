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
        return targets.Select(static (s, _) => OutputFiles(s.Left, s.Right));
    }

    private static ImmutableArray<CodeOutput> OutputFiles(
        ImmutableArray<MemberTargetAndTemplateData> memberTemplates,
        ImmutableArray<AssemblyTargetAndTemplateData> assemblyTemplates)
    {
        var codeOut = ImmutableArray.CreateBuilder<CodeOutput>();

        var fileGroups = memberTemplates.GroupBy(x => (x.TemplateType, x.TargetClass, x.GeneratorName));

        foreach (var group in fileGroups)
        {
            var fileContents = string.Empty;
            if (group.Key.TemplateType == TemplateType.Complete)
            {
                var sb = new StringBuilder();
                sb.AppendLine(@$"
                    namespace {group.Key.TargetClass.NamespaceName}
                    {{
                        public partial class {group.Key.TargetClass.ClassName}
                        {{");

                foreach (var templates in group.AsEnumerable())
                {
                    sb.AppendLine(templates.Code);
                }

                sb.AppendLine(@"
                        }
                    }");
                fileContents = sb.ToString();
            }
            else if (group.Key.TemplateType == TemplateType.File)
            {
                fileContents = group.First().Code; // TODO there should never be more than one.
            }

            var fullCode = CSharpSyntaxTree.ParseText(fileContents).GetRoot().NormalizeWhitespace().ToFullString();
            var hintName = $"{group.Key.TargetClass.ClassName}_{group.Key.GeneratorName}.g.cs";
            codeOut.Add(new CodeOutput(hintName, fullCode));
        }

        foreach (var assemblyTemplate in assemblyTemplates)
        {
            var hintName = $"{assemblyTemplate.GlobalGeneratorName}.g.cs";
            var renderData = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();
            var renderedTemplate = Mustache.RenderTemplate(assemblyTemplate.GlobalTemplate, renderData.ToImmutable());
            var fullCode = CSharpSyntaxTree.ParseText(renderedTemplate).GetRoot().NormalizeWhitespace().ToFullString();

            codeOut.Add(new CodeOutput(hintName, fullCode));
        }

        return codeOut.ToImmutable();
    }
}
