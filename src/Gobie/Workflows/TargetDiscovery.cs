using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gobie.Enums;

namespace Gobie.Workflows;

public class TargetDiscovery
{
    public static IncrementalValuesProvider<ClassDeclarationSyntax> FindClassesWithAttributes(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static x => x is not null)!;
    }

    /// <summary>
    /// Tries to find targets without relying on any complex operations.
    /// </summary>
    public static IncrementalValuesProvider<(ClassDeclarationSyntax, ImmutableArray<UserGeneratorTemplateData>)?> FindProbableTargets(
        IncrementalValuesProvider<(ClassDeclarationSyntax Left, ImmutableArray<UserGeneratorTemplateData> Right)> cwaAndGenerators)
    {
        return cwaAndGenerators.Select(selector: static (s, _) => FindProbableTargets(s.Left, s.Right));
    }

    public static IncrementalValuesProvider<DataOrDiagnostics<ImmutableArray<TargetAndTemplateData>>> GetTargetsOrDiagnostics(IncrementalValuesProvider<((ClassDeclarationSyntax, ImmutableArray<UserGeneratorTemplateData>)? Left, Compilation Right)> data)
    {
        return data
            .Select(selector: static (s, _) =>
                GetTargetsOrDiagnostics(s.Left?.Item1, s.Left?.Item2, s.Right));
    }

    private static DataOrDiagnostics<ImmutableArray<TargetAndTemplateData>> GetTargetsOrDiagnostics(
        ClassDeclarationSyntax? cds,
        ImmutableArray<UserGeneratorTemplateData>? templates,
        Compilation compilation)
    {
        var output = new List<TargetAndTemplateData>();
        var diagnostics = new List<Diagnostic>();

        if (cds is null || templates is null)
        {
            return new DataOrDiagnostics<ImmutableArray<TargetAndTemplateData>>(diagnostics);
        }

        // Verify for certian this is a target
        var sm = compilation.GetSemanticModel(cds.SyntaxTree);
        var typeInfo = sm.GetDeclaredSymbol(cds);

        if (typeInfo is null)
        {
            // TODO should this change?
            return new DataOrDiagnostics<ImmutableArray<TargetAndTemplateData>>(diagnostics);
        }

        foreach (var att in typeInfo.GetAttributes())
        {
            var a = att.AttributeClass;
            var b = a.ContainingNamespace.Name; // TODO this could be a global namespace which is an empty string
            var ctypeName = a.Name;

            foreach (var template in templates)
            {
                if (ctypeName == template.AttributeData.DefinitionIdentifier)
                {
                    var ti = typeInfo.Name;
                    var tin = typeInfo.ContainingNamespace.Name;
                    var code = string.Join(Environment.NewLine, template.Templates);
                    output.Add(
                        new TargetAndTemplateData(
                            TemplateType.Complete,
                            ctypeName,
                            new TargetClass(ti, tin),
                            code));
                }
            }
        }

        // Get attribute data

        // Match it to a template. if it matches, get the ctor and named data off the attribute.

        // TODO later: Get the data off the target attributes

        // Output some object that can be rendered into source code. We do this as multiple steps to
        // support global templates down the road.

        var builder = ImmutableArray.CreateBuilder<TargetAndTemplateData>();
        builder.AddRange(output);
        return new DataOrDiagnostics<ImmutableArray<TargetAndTemplateData>>(builder.ToImmutable());
    }

    private static (ClassDeclarationSyntax, ImmutableArray<UserGeneratorTemplateData>)? FindProbableTargets(
            ClassDeclarationSyntax cds,
        ImmutableArray<UserGeneratorTemplateData> userGenerators)
    {
        foreach (var item in cds.AttributeLists.SelectMany(x => x.Attributes))
        {
            foreach (var gen in userGenerators)
            {
                var a = ((IdentifierNameSyntax)item.Name).Identifier.Text;
                if (userGenerators.Any(x => x.AttributeData.AttributeIdentifier == a + "Attribute"))
                {
                    return (cds, userGenerators);
                }
            }
        }

        return null;
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0;
    }
}
