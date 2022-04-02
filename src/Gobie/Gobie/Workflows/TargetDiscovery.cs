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
            var ctypeName = a.Name + (a.Name.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) ? "" : "Attribute");

            foreach (var template in templates)
            {
                if (ctypeName == template.AttributeData.AttributeIdentifier.ClassName)
                {
                    var ti = typeInfo.Name;
                    var tin = typeInfo.ContainingNamespace.Name;

                    // Based on the output of the diagnostic below it looks like we aren't able to
                    // get the attribute args. This doesn't seem to be isolated to unit testing.
                    // Even in the console client we find zero args when we follow the same process
                    // we use to get args for required position or generator name. My current guess
                    // now is that the semantic model above doesn't (and maybe cannot) have the
                    // definitions of the attributes we create in the generator. (I'm assuming the
                    // register post generation initaliztion code is doign something different,
                    // because we were able to get the constructor args for those).
                    diagnostics.Add(Diagnostic.Create(Diagnostics.Errors.GobieUnknownError($"Attribute {ctypeName} has {att.ConstructorArguments.Length} CtorArgs and {att.NamedArguments.Length} NamedArgs"), null));

                    var data = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();
                    if (att.ConstructorArguments.Length > 0)
                    {
                        if (att.ConstructorArguments[0].Value is int o)
                        {
                            data.Add("Num1", new Mustache.RenderData("Num1", o.ToString(), true));
                        }
                        else
                        {
                            // Here some arg exists but it isn't an int so the compiler should be
                            // erroring. So we just return diagnostics if any and stop.
                            return new(diagnostics);
                        }
                    }
                    if (att.ConstructorArguments.Length > 1)
                    {
                        if (att.ConstructorArguments[1].Value is int o)
                        {
                            data.Add("Num2", new Mustache.RenderData("Num2", o.ToString(), true));
                        }
                        else
                        {
                            // Here some arg exists but it isn't an int so the compiler should be
                            // erroring. So we just return diagnostics if any and stop.
                            return new(diagnostics);
                        }
                    }

                    // Output some object that can be rendered into source code. We do this as
                    // multiple steps to support global templates down the road.

                    var templateData = data.ToImmutable();

                    var sb = new StringBuilder();

                    foreach (var t in template.Templates)
                    {
                        sb.AppendLine(Mustache.RenderTemplate(t, templateData));
                        sb.AppendLine();
                    }

                    output.Add(
                        new TargetAndTemplateData(
                            TemplateType.Complete,
                            ctypeName,
                            new ClassIdentifier(tin, ti),
                            sb.ToString()));
                }
            }
        }

        var builder = ImmutableArray.CreateBuilder<TargetAndTemplateData>();
        builder.AddRange(output);
        return new DataOrDiagnostics<ImmutableArray<TargetAndTemplateData>>(builder.ToImmutable(), diagnostics);
    }

    private static (ClassDeclarationSyntax, ImmutableArray<UserGeneratorTemplateData>)? FindProbableTargets(
            ClassDeclarationSyntax cds,
        ImmutableArray<UserGeneratorTemplateData> userGenerators)
    {
        var n = cds.ToFullString();
        foreach (var item in cds.AttributeLists.SelectMany(x => x.Attributes))
        {
            var classAttName = ((IdentifierNameSyntax)item.Name).Identifier.Text;
            classAttName += classAttName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) ? "" : "Attribute";

            foreach (var gen in userGenerators)
            {
                var genAttName = gen.AttributeData.AttributeIdentifier.ClassName;
                if (genAttName == classAttName)
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
