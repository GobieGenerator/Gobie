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

        var ti = typeInfo.Name;
        var tin = typeInfo.ContainingNamespace.Name;

        // It looks like we aren't able to get the attribute args off of SourceAttributeData (from
        // typeInfo.GetAttributes()). This doesn't seem to be isolated to unit testing. Even in the
        // console client we find zero args when we follow the same process we use to get args for
        // required position or generator name. My current guess now is that the semantic model
        // above doesn't (and maybe cannot) have the definitions of the attributes we create in the
        // generator. (I'm assuming the register post generation initaliztion code is doign
        // something different, because we were able to get the constructor args for those).
        // Additionally I noticed that the SourceAttributeData (att) is missing the namespace and
        // doesn't say attribute at the end. So this seems like what happened when the unit tests
        // were missing a reference.
        foreach (var att in cds.AttributeLists.SelectMany(x => x.Attributes))
        {
            // TODO, maybe we should test that we can't resolve the specific attribute details and
            // then look to the syntax? I wonder if we define a generator in one lib and use it in
            // another whether that is even viable. And in that case we might be able to see the
            // full class when the dependant lib compiles.
            var attName = att.Name.ToFullString();
            var ctypeName = attName + (attName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) ? "" : "Attribute");

            foreach (var template in templates)
            {
                if (ctypeName == template.AttributeData.AttributeIdentifier.ClassName)
                {
                    var data = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();

                    if (att.ArgumentList is not null)
                    {
                        for (int i = 0; i < att.ArgumentList.Arguments.Count; i++)
                        {
                            var arg = att.ArgumentList.Arguments[i];
                            var constValArg = sm.GetConstantValue(arg.Expression);
                            if (arg.NameEquals is null && constValArg.HasValue && i < template.AttributeData.RequiredParameters.Count())
                            {
                                // This is a required argument either with or without colon equals
                                var ident = template.AttributeData.RequiredParameters.ElementAt(i).NamePascal;
                                data.Add(
                                    ident,
                                    new Mustache.RenderData(ident, constValArg.Value!.ToString(), true));
                            }
                            else if (arg.NameEquals is not null && constValArg.HasValue)
                            {
                                // This is a named parameter (i.e. optional value prefixed by 'Name
                                // = '
                                var n = arg.NameEquals.Name.ToFullString().Trim();
                                data.Add(
                                   n,
                                   new Mustache.RenderData(n, constValArg.Value!.ToString(), true));
                            }
                        }

                        if (data.Count < template.AttributeData.RequiredParameters.Count())
                        {
                            for (int i = data.Count; i < template.AttributeData.RequiredParameters.Count(); i++)
                            {
                                var param = template.AttributeData.RequiredParameters.ElementAt(i);
                                var ident = param.NamePascal;
                                data.Add(
                                    ident,
                                    new Mustache.RenderData(ident, param.InitalizerLiteral, true));
                            }
                        }
                    }

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
