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
        return incrementalValueProvider.Select(static (s, _) => Asdf(s));
    }

    public static ImmutableArray<CodeOutput> Asdf(
        ImmutableArray<TargetAndTemplateData> compliationAndGeneratorDeclarations)
    {
        var codeOut = ImmutableArray.CreateBuilder<CodeOutput>();

        var fileGroups = compliationAndGeneratorDeclarations.GroupBy(x => (x.TargetClass, x.GeneratorName));

        foreach (var group in fileGroups)
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

            var fullCode = CSharpSyntaxTree.ParseText(sb.ToString()).GetRoot().NormalizeWhitespace().ToFullString();
            var hintName = $"{group.Key.GeneratorName}_{group.Key.TargetClass.ClassName}.g.cs";
            codeOut.Add(new CodeOutput(hintName, fullCode));
        }

        return codeOut.ToImmutable();
    }
}
