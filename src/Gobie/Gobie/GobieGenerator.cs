namespace Gobie;

[Generator]
public class GobieGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find the user templates and report diagnostics on issues.
        var userTemplateSyntaxOrDiagnostics = GeneratorDiscovery.FindUserTemplates(context);
        DiagnosticsReporting.Report(context, userTemplateSyntaxOrDiagnostics);
        var userTemplateSyntax = ExtractData(userTemplateSyntaxOrDiagnostics);

        GeneratorDiscovery.GenerateAttributes(context, userTemplateSyntax);

        var compliationAndGeneratorDeclarations = userTemplateSyntax.Combine(context.CompilationProvider);
        var userGeneratorsOrDiagnostics = GeneratorDiscovery.GetFullGenerators(compliationAndGeneratorDeclarations);
        DiagnosticsReporting.Report(context, userGeneratorsOrDiagnostics);
        var userGenerators = ExtractData(userGeneratorsOrDiagnostics);

        // Target Discovery Workflow
        var mwa = TargetDiscovery.FindMembersWithAttributes(context);
        var mwaAndGenerators = mwa.Combine(userGenerators.Collect());
        var probableTargets = TargetDiscovery.FindProbableTargets(mwaAndGenerators);

        var compliationAndProbableTargets = probableTargets.Where(x => x is not null).Combine(context.CompilationProvider);
        var targetsOrDiagnostics = TargetDiscovery.GetTargetsOrDiagnostics(compliationAndProbableTargets);
        DiagnosticsReporting.Report(context, targetsOrDiagnostics);
        var targets = ExtractManyData(targetsOrDiagnostics);

        // Consolodate outputs down to files and output them.
        IncrementalValueProvider<ImmutableArray<CodeOutput>> codeOut = CodeGeneration.CollectOutputs(targets.Collect());
        context.RegisterSourceOutput(codeOut, static (spc, source) => CodeGeneration.Output(spc, source));
    }

    private static IncrementalValuesProvider<T> ExtractData<T>(IncrementalValuesProvider<DataOrDiagnostics<T>> values)
    {
        return values
            .Where(x => x.Data is not null)
            .Select(selector: (s, _) => s.Data!);
    }

    private static IncrementalValuesProvider<T> ExtractManyData<T>(IncrementalValuesProvider<DataOrDiagnostics<ImmutableArray<T>>> values)
    {
        return values
            .SelectMany(selector: (s, _) => s.Data);
    }
}
