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
        // This method is responsible for mapping the text from the Constant Expression, which is
        // the text the user intended to be output after escape characters or similar, and point
        // back to the orginal text so we can point at exact errors within the source code.

        // TODO handle c#7 strings.

        var literal = false;
        var inQuotes = false;

        var cs = fullText.AsSpan();
        var stringContentsStart = 0;
        for (int i = 0; i < cs.Length; i++)
        {
            if (!inQuotes && cs[i] == '@')
            {
                literal = true;
            }
            else if (!inQuotes && cs[i] == '"')
            {
                stringContentsStart = i + 1;
                break;
            }
        }

        return Location.Create(tree, new TextSpan(fullSpan.Start + stringContentsStart + start, len));
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
