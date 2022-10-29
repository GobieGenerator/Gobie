namespace Gobie.Models.Templating;

/// <summary>
/// This class should store the template text itself, and provide the ability to figure out the location...
/// </summary>
public readonly struct TemplateText
{
    private static readonly char[] newLine = new char[] { '\n' };
    private readonly string constantText;
    private readonly string fullText;
    private readonly SyntaxTree tree;
    private readonly SyntaxKind syntaxKind;

    private readonly TextSpan span;

    public TemplateText(LiteralExpressionSyntax literalExpressionSyntax, string constantText)
    {
        this.constantText = constantText;
        tree = literalExpressionSyntax.SyntaxTree;
        fullText = literalExpressionSyntax.Token.Text;
        syntaxKind = literalExpressionSyntax.Token.Kind();
        span = literalExpressionSyntax.Span; //Span, but excluding leading trivia.
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

        return syntaxKind switch
        {
            SyntaxKind.SingleLineRawStringLiteralToken => GetLocationAtFromSingleLineRaw(start, len),
            SyntaxKind.MultiLineRawStringLiteralToken => GetLocationAtFromMultiLineRaw(start, len),
            _ => GetLocationAtFromNormalString(start, len),
        };
    }

    /// <summary>
    /// Returns the location of the text from <see cref="Text"/>, along with leading and trailing
    /// quotes, @ symbols...
    /// </summary>
    public Location GetLocation()
    {
        return Location.Create(tree, span);
    }

    private static bool IsEscape(ReadOnlySpan<char> chars) => chars switch
    {
        @"\'" => true,
        @"\""" => true,
        @"\\" => true,
        @"\0" => true,
        @"\a" => true,
        @"\b" => true,
        @"\f" => true,
        @"\n" => true,
        @"\r" => true,
        @"\t" => true,
        @"\v" => true,
        // For now at least, deliberatly ignoring unicode escape chars.
        _ => false,
    };

    private Location GetLocationAtFromSingleLineRaw(int start, int len)
    {
        // We start at 3, becaue raw single line must begin with at least 3 quotes.
        for (var i = 3; i < fullText.Length; i++)
        {
            if (fullText[i] != '"')
            {
                return Location.Create(tree, new TextSpan(span.Start + start + i, len));
            }
        }

        return GetLocation();
    }

    private Location GetLocationAtFromMultiLineRaw(int start, int len)
    {
        var a = fullText.Split(newLine, StringSplitOptions.None);
        var padding = 0;
        var lastLine = a.Last();
        for (var i = 0; i < lastLine.Length; i++)
        {
            if (lastLine[i] == '"')
            {
                padding = i;
                break;
            }
        }

        return Location.Create(tree, new TextSpan(span.Start + start + a[0].Length + 1 + padding, len));
    }

    private Location GetLocationAtFromNormalString(int start, int len)
    {
        var isVerbatim = false;
        var inQuotes = false;

        var cs = fullText.AsSpan();
        var stringContentsStart = 0;
        for (int i = 0; i < cs.Length; i++)
        {
            if (!inQuotes && cs[i] == '@')
            {
                isVerbatim = true;
            }
            else if (!inQuotes && cs[i] == '"')
            {
                stringContentsStart = i + 1;
                break;
            }
        }

        var escapeCharCount = 0;
        if (isVerbatim == false)
        {
            for (int i = stringContentsStart; i < cs.Length; i++)
            {
                if (cs[i] == '\\' && IsEscape(cs.Slice(i, 2)))
                {
                    // We found an escaped char in a normal string.
                    i++;
                    escapeCharCount++;
                }

                if (i + escapeCharCount >= start)
                {
                    break; // We are done looking for escape chars
                }
            }
        }

        // This is right so long as the wrapped text does NOT have escaped char in it.
        return Location.Create(tree, new TextSpan(span.Start + stringContentsStart + start + escapeCharCount, len));
    }
}
