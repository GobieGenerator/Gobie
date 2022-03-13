using Microsoft.CodeAnalysis;

namespace Gobie.Workflows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CodeGeneration
{
    public static void Output(SourceProductionContext spc, string source)
    {
        spc.AddSource("afile.g.cs", source);
    }
}
