using Microsoft.CodeAnalysis;

namespace Gobie.Workflows;

using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CodeGeneration
{
    public static void Output(SourceProductionContext spc, TargetAndTemplateData source)
    {
        var fullCode = $@"
            namespace MyNamespace
            {{
                public partial class {source.TargetName}
                {{
                    {source.Code}
                }}
            }}
            ";

        fullCode = CSharpSyntaxTree.ParseText(fullCode).GetRoot().NormalizeWhitespace().ToFullString();

        spc.AddSource($"{source.GeneratorName}_{source.TargetName}.g.cs", fullCode);
    }
}
