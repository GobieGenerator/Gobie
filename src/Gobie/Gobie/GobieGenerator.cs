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

        // TODO look for gobie settings coming from attributes
        var assemblyAtt = AssemblyAttributes.FindAssemblyAttributes(context);

        context.RegisterSourceOutput(assemblyAtt, static (spc, source) =>
        {
            Console.WriteLine("");
        });

        // ========== Target Discovery Workflow ================
        // First: Discover classes and field targets.
        var mwa = TargetDiscovery.FindMembersWithAttributes(context);
        var mwaAndGenerators = mwa.Combine(userGenerators.Collect());
        var probableTargets = TargetDiscovery.FindProbableTargets(mwaAndGenerators);
        var compliationAndProbableTargets = probableTargets.Where(x => x is not null).Combine(context.CompilationProvider);
        var targetsOrDiagnostics = TargetDiscovery.GetTargetsOrDiagnostics(compliationAndProbableTargets);
        DiagnosticsReporting.Report(context, targetsOrDiagnostics);
        var memberTargets = ExtractManyData(targetsOrDiagnostics);

        // Second: Discover assembly targets (i.e. Requests for global template gen).
        var assemblyAttributesAndGenerators = assemblyAtt.Combine(userGenerators.Collect());
        var probableAssemblyTargets = TargetDiscovery.FindProbableAssemblyTargets(assemblyAttributesAndGenerators);
        var compliationAndProbableAssemblyTargets = probableAssemblyTargets.Where(x => x is not null).Combine(context.CompilationProvider);
        var assemblyTargetsOrDiagnostics = TargetDiscovery.GetAssemblyTargetsOrDiagnostics(compliationAndProbableAssemblyTargets);
        DiagnosticsReporting.Report(context, assemblyTargetsOrDiagnostics);
        var assemblyTargets = ExtractData(assemblyTargetsOrDiagnostics);

        // =========== Code Generation Workflow ==============

        // First: Concatenate the target types into a single output.
        var targetsTuple = memberTargets.Collect().Combine(assemblyTargets.Collect());
        var targets = targetsTuple.Select(static (f, _) =>
        {
            var builder = ImmutableArray.CreateBuilder<TargetAndTemplateData>();
            builder.AddRange(f.Left);
            builder.AddRange(f.Right);
            return builder.ToImmutable();
        });

        // Consolidate outputs down to files and output them.
        IncrementalValueProvider<ImmutableArray<CodeOutput>> codeOut = CodeGeneration.CollectOutputs(targets);
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
