using System.Collections.Immutable;

namespace Gobie.Workflows;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        return cwaAndGenerators
                    .Select(selector: static (s, _) => FindProbableTargets(s.Left, s.Right));
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
