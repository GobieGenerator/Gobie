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
        IncrementalValueProvider<ImmutableArray<TargetAndTemplateData>> incrementalValueProvider)
    {
        return incrementalValueProvider.Select(static (s, _) => OutputFiles(s));
    }

    public static ImmutableArray<CodeOutput> OutputFiles(
        ImmutableArray<TargetAndTemplateData> compliationAndGeneratorDeclarations)
    {
        var codeOut = ImmutableArray.CreateBuilder<CodeOutput>();

        var fileGroups = compliationAndGeneratorDeclarations.GroupBy(x => (x.TemplateType, x.TargetClass, x.GeneratorName));

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

        return codeOut.ToImmutable();
    }
}
