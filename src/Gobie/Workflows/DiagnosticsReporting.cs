namespace Gobie.Workflows;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DiagnosticsReporting
{
    public static void Report<T>(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<DataOrDiagnostics<T>> diagnostics)
    {
        context.RegisterSourceOutput(diagnostics, static (spc, source) => Report(spc, source));
    }

    private static void Report<T>(SourceProductionContext spc, DataOrDiagnostics<T> option)
    {
        if (option.Diagnostics is not null)
            foreach (var diagnostic in option.Diagnostics)
                spc.ReportDiagnostic(diagnostic);
    }
}
