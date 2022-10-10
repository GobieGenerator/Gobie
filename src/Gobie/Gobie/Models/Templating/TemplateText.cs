namespace Gobie.Models.Templating;

/// <summary>
/// This class should store the template text itself, and provide the ability to figure out the location...
/// </summary>
public readonly struct TemplateText
{
    private readonly string constantText;
    private readonly SyntaxTree tree;

    /// <summary>
    /// Full text includes enclosing quotes, @ symbols and can even include escaped characters like
    /// /n which are actually a new line in the constantText.
    /// </summary>
    private readonly string fullText;

    private readonly TextSpan fullSpan;

    public TemplateText(LiteralExpressionSyntax literalExpressionSyntax, string constantText)
    {
        this.constantText = constantText;
        tree = literalExpressionSyntax.SyntaxTree;
        fullText = literalExpressionSyntax.Token.Text;
        fullSpan = literalExpressionSyntax.FullSpan;
    }

    public string Text => constantText;

    /// <summary>
    /// Returns the location of text within <see cref="Text"/>.
    /// </summary>
    public Location GetLocationAt(int start, int len)
    {
        return Location.Create(tree, new TextSpan(fullSpan.Start + start, len));
    }

    /// <summary>
    /// Returns the location of the text from <see cref="Text"/>, along with leading and trailing
    /// quotes, @ symbols...
    /// </summary>
    public Location GetLocation()
    {
        return Location.Create(tree, fullSpan);
    }
}
