namespace Gobie.Workflows;

public static class AssemblyAttributes
{
    public static IncrementalValuesProvider<AttributeSyntax> FindAssemblyAttributes(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsAssemblyAttribute(s),
                transform: static (ctx, _) => (AttributeSyntax)ctx.Node)
            .Where(static x => x is not null)!;
    }

    private static bool IsAssemblyAttribute(SyntaxNode node)
    {
        return (node is AttributeSyntax c &&
               c.Parent is AttributeListSyntax als &&
               als.Target?.Identifier.ToFullString() == "assembly");
    }
}
