namespace Gobie.Models.Templating;

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

public class Mustache
{
    public enum TokenType
    {
        None = 0,
        Close,
        LogicIfOpen,
        LogicNotOpen,
        LogicEndOpen,
        General,

        /// <summary>
        /// This is the start of a token with an identifier that is rendered.
        /// </summary>
        TemplateTokenOpen,

        /// <summary>
        /// This token is comprised of char that can be an identifier.
        /// </summary>
        Identifier,

        Whitespace,
        Colon,
    }

    public enum TemplateSyntaxType
    {
        Root,
        Literal,
        If,
        Not,
        Identifier,
    }

    public enum FormatSetting
    {
        None = 0,
        Camel,
        Pascal,
    }

    private enum IdentifierValidity
    {
        Valid,
        IdentifierUsedWhereItDoesntExist,
        IdentifierExcludedWhereItDoesExist,
    }

    public static ReadOnlySpan<Token> Tokenize(ReadOnlySpan<char> template)
    {
        var tokens = new Span<Token>(new Token[template.Length]);

        // Not we start at -1 so we can differentiate the first time we add a token from the first
        // time we might be combining tokens.
        int t = -1;

        for (int i = 0; i < template.Length; i++)
        {
            // Take advantage of language checking every one of these till it hits one.
            if (TryMatch(tokens, template, "{{#", TokenType.LogicIfOpen)
                || TryMatch(tokens, template, "{{^", TokenType.LogicNotOpen)
                || TryMatch(tokens, template, "{{/", TokenType.LogicEndOpen)
                || TryMatch(tokens, template, "{{", TokenType.TemplateTokenOpen)
                || TryMatch(tokens, template, "}}", TokenType.Close))
            {
            }
            else if (template[i] == ':')
            {
                AddOrCombine(tokens, new Token(i, i, TokenType.Colon));
            }
            else if (char.IsWhiteSpace(template[i]))
            {
                AddOrCombine(tokens, new Token(i, i, TokenType.Whitespace));
            }
            else if (char.IsLetterOrDigit(template[i]) || template[i] == '_')
            {
                AddOrCombine(tokens, new Token(i, i, TokenType.Identifier));
            }
            else
            {
                AddOrCombine(tokens, new Token(i, i, TokenType.General));
            }

            bool TryMatch(Span<Token> tokens, ReadOnlySpan<char> template, string compare, TokenType tokenType)
            {
                var len = compare.Length;
                if (i + len > template.Length)
                {
                    return false;
                }

                var matches = MemoryExtensions
                    .Equals(
                        template.Slice(i, len),
                        compare.AsSpan(),
                        StringComparison.OrdinalIgnoreCase);

                if (matches == false)
                {
                    return false;
                }

                t++;
                tokens[t] = new Token(i, i + len - 1, tokenType);
                i += compare.Length - 1; // Advance, but keep in mind we are going to still get i++'
                return true;
            }
        }

        void AddOrCombine(Span<Token> tokens, Token token)
        {
            if (t == -1)
            {
                t++;
                tokens[t] = token;
            }
            else if (token.TokenType == tokens[t].TokenType)
            {
                tokens[t] = new Token(tokens[t].Start, token.End, tokens[t].TokenType);
            }
            else
            {
                // Here we have a token we can't combine
                t++;
                tokens[t] = token;
            }
        }

        return tokens.Slice(0, t + 1);
    }

    public static DataOrDiagnostics<TemplateDefinition> Parse(ReadOnlySpan<char> template, Func<int, int, Location>? initialLocation)
    {
        var tokens = Tokenize(template);
        var root = new TemplateSyntax(null, TemplateSyntaxType.Root, string.Empty, string.Empty, FormatSetting.None);
        var diagnostics = new List<Diagnostic>();
        var identifiers = ImmutableHashSet.CreateBuilder<string>();

        string GetText(ReadOnlySpan<char> template, Token token)
        {
            return template.Slice(token.Start, token.End - token.Start + 1).ToString();
        }

        var currentNode = root;

        for (int i = 0; i < tokens.Length; i++)
        {
            if (currentNode is null)
            {
                break; // Something is wrong so stop.
            }

            bool TokenIsText(ReadOnlySpan<Token> tokens, int i) => tokens[i].TokenType is TokenType.General or TokenType.Identifier or TokenType.Whitespace or TokenType.Colon;
            if (TokenIsText(tokens, i))
            {
                var sb = new StringBuilder();
                sb.Append(GetText(template, tokens[i]));

                // Consolidate literal tokens. Might not be the absolute fastest option, but it will
                // generate a simpler AST.
                while (i + 1 < tokens.Length && TokenIsText(tokens, i + 1))
                {
                    i++;
                    sb.Append(GetText(template, tokens[i]));
                }

                currentNode.Children.Add(new TemplateSyntax(currentNode, TemplateSyntaxType.Literal, sb.ToString(), string.Empty, FormatSetting.None));
            }
            else if (tokens[i].TokenType is TokenType.Close)
            {
                diagnostics.Add(
                    Diagnostic.Create(
                        Diagnostics.UnexpectedToken(
                            GetText(template, tokens[i]),
                            "There is no corresponding open token"),
                        null));
            }
            else
            {
                // Here, the only remaining types are opening tokens, which should be followed by
                // either 2 or 4 additional tokens in the following patterns: 'Identifier Close' or
                // 'Identifier Colon Identifier[Matches format token] Close'
                var tagClosed = false;
                var identifier = string.Empty;
                var formatSetting = FormatSetting.None;
                var initialToken = tokens[i];

                var upcommingTokens = PeekNonWhitespace(tokens, i);

                // Move the next loop forward, because we have handled the following number of upcommingTokens.
                void AdvanceNextToken(ReadOnlySpan<IndexedToken> upcomming, int upcommingTokensHandled) => i = upcomming[upcommingTokensHandled - 1].Index;

                if (initialToken.TokenType == TokenType.TemplateTokenOpen &&
                    TokenKindsMatch(upcommingTokens, TokenType.Identifier, TokenType.Colon, TokenType.Identifier, TokenType.Close))
                {
                    tagClosed = true;
                    identifier = GetText(template, upcommingTokens[0].Token);

                    var formatToken = upcommingTokens[2].Token;
                    var formatTokenText = GetText(template, formatToken);
                    AdvanceNextToken(upcommingTokens, 4);

                    if (IdentifierIsFormatToken(formatTokenText, out FormatSetting f))
                    {
                        // We do have a valid formatted identifier
                        formatSetting = f;
                    }
                    else
                    {
                        diagnostics.Add(
                            Diagnostic.Create(
                                Diagnostics.InvalidFormatToken(formatTokenText),
                                initialLocation?.Invoke(formatToken.Start, formatToken.End - formatToken.Start + 1)));
                    }
                }
                else if (initialToken.TokenType == TokenType.TemplateTokenOpen &&
                    TokenKindsMatch(upcommingTokens, TokenType.Identifier, TokenType.Identifier, TokenType.Close))
                {
                    // Looks like {{ a b }}. Either missing a colon or two
                    var formatToken = GetText(template, upcommingTokens[1].Token);
                    AdvanceNextToken(upcommingTokens, 3);

                    if (IdentifierIsFormatToken(formatToken, out FormatSetting _))
                    {
                        // We should just be missing a colon
                        diagnostics.Add(
                            Diagnostic.Create(
                                Diagnostics.MissingToken(":"),
                                null));
                    }
                    else
                    {
                        // Not sure what is wrong
                        diagnostics.Add(
                            Diagnostic.Create(
                                Diagnostics.UnexpectedToken(
                                    GetText(template, upcommingTokens[1].Token),
                                    "This is either missing a colon or has a second unexpected identifier"),
                                null));
                    }
                }
                else if (initialToken.TokenType == TokenType.TemplateTokenOpen &&
                    TokenKindsMatch(upcommingTokens, TokenType.Identifier, TokenType.Colon, TokenType.Identifier))
                {
                    if (upcommingTokens.Length == 4)
                    {
                        // We have an unexpected token where we expect the closing token
                        AddExpectedClosingToken(diagnostics, GetText(template, upcommingTokens[3].Token));
                    }
                    else
                    {
                        // Tokens ended and we are missing the closing token.
                        AddMissingToken(diagnostics, "}}");
                    }
                }
                else if (TokenKindsMatch(upcommingTokens, TokenType.Identifier, TokenType.Close))
                {
                    // We have a complete and finalized token
                    tagClosed = true;
                    AdvanceNextToken(upcommingTokens, 2);
                    identifier = GetText(template, upcommingTokens[0].Token);
                }
                else if (TokenKindsMatch(upcommingTokens, TokenType.Identifier))
                {
                    // Here we know the upcomming tokens are an identifier and then NOT a close, b/c of above case.
                    if (TokenKindSeek(upcommingTokens, TokenType.Close, TokenType.TemplateTokenOpen, out var closeToken))
                    {
                        tagClosed = true;
                        i = closeToken.Index + 1;
                        diagnostics.Add(
                            Diagnostic.Create(
                                Diagnostics.TemplateTagIsInvalid(),
                                initialLocation?.Invoke(initialToken.Start, closeToken.Token.End - initialToken.Start + 1)));
                    }
                    else if (upcommingTokens.Length == 2)
                    {
                        // No closing token exists, and we have an unexpected token where we expect the closing token
                        AddExpectedClosingToken(diagnostics, GetText(template, upcommingTokens[1].Token));
                    }
                    else
                    {
                        // Tokens ended and we are missing the closing token.
                        AddMissingToken(diagnostics, "}}");
                    }
                }
                else if (TokenKindsMatch(upcommingTokens, TokenType.Close))
                {
                    tagClosed = true;
                    AdvanceNextToken(upcommingTokens, 1);
                    diagnostics.Add(
                        Diagnostic.Create(
                            Diagnostics.TemplateTagIsEmpty(),
                            initialLocation?.Invoke(initialToken.Start, upcommingTokens[0].Token.End - initialToken.Start + 1)));
                }
                else if (upcommingTokens.Length == 1)
                {
                    // The first token should be an identifier but isn't.
                    AdvanceNextToken(upcommingTokens, 1);
                    diagnostics.Add(
                        Diagnostic.Create(
                            Diagnostics.UnexpectedToken(
                                GetText(template, upcommingTokens[0].Token),
                                "Expected an identifier string, which contains only letters, numbers, and underscores"),
                            null));
                }
                else
                {
                    // Else, this is just an opening token.
                    AddTemplateIncomplete(diagnostics);
                }

                if (tagClosed && initialToken.TokenType != TokenType.LogicEndOpen)
                {
                    // Here we found a complete valid tag for an identifier or an opening logical
                    // tag. First, we verify the identifier makes sense in this context (i.e. no
                    // current or parent node is a NOT with the same identifier, meaning this will
                    // never render). If thats ok we can go ahead and create a new syntax node. If
                    // this tag represents the opening of a logical node, then that becomes the new
                    // current node.

                    var tst = initialToken.TokenType switch
                    {
                        TokenType.TemplateTokenOpen => TemplateSyntaxType.Identifier,
                        TokenType.LogicIfOpen => TemplateSyntaxType.If,
                        TokenType.LogicNotOpen => TemplateSyntaxType.Not,
                        _ => throw new InvalidOperationException("Shouldn't be possible"),
                    };

                    var identValid = IdentiferValidInThisContext(tst, currentNode, identifier);
                    if (identValid == IdentifierValidity.IdentifierUsedWhereItDoesntExist)
                    {
                        diagnostics.Add(
                            Diagnostic.Create(
                                Diagnostics.UnreachableTemplateSection(
                                    $"You cannot use the identifier '{identifier}' here because it is surrounded " +
                                    $"by a not node (i.e. {{{{^{identifier}}}}} )"),
                                null));
                    }
                    else if (identValid == IdentifierValidity.IdentifierExcludedWhereItDoesExist)
                    {
                        diagnostics.Add(
                           Diagnostic.Create(
                              Diagnostics.UnreachableTemplateSection(
                                  $"You cannot exclude the identifier '{identifier}' here because it is surrounded " +
                                  $"by an if node (i.e. {{{{#{identifier}}}}} )"),
                              null));
                    }

                    // We close the syntax even if identifier isn't valid, b/c we don't want
                    // inaccurate alerts saying the template is incomplete.
                    identifiers.Add(identifier);
                    var ts = new TemplateSyntax(currentNode, tst, string.Empty, identifier, formatSetting);
                    currentNode.Children.Add(ts);
                    currentNode = ts.Type == TemplateSyntaxType.Identifier ? currentNode : ts;
                }
                if (tagClosed && initialToken.TokenType == TokenType.LogicEndOpen)
                {
                    // Here we found a complete valid tag which ends a logical tag. We need to check
                    // if the currentNode is the matching opening tag (the good case) and move the
                    // current node back to be its parent. If not we need to report diagnostics.

                    if (currentNode.Type is TemplateSyntaxType.If or TemplateSyntaxType.Not)
                    {
                        if (currentNode.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                        {
                            // The node matches, so we close it, by navigating back to the parent
                            currentNode = currentNode.Parent;
                        }
                        else
                        {
                            // The node doesn't match at all, so show what we expect.
                            diagnostics.Add(
                               Diagnostic.Create(
                                   Diagnostics.LogicalEndMissing("{{\\" + currentNode.Identifier + "}}"),
                                   null));
                        }
                    }
                    else
                    {
                        // Here, we aren't able to close a node. So we issue a diagnostic.
                        diagnostics.Add(
                            Diagnostic.Create(
                                Diagnostics.UnexpectedToken(
                                    GetText(template, tokens[i]),
                                    "There is no corresponding if or not tag (i.e. {{#name}} or {{^name}} for this to close"),
                                null));
                    }
                }
                else
                {
                    // TODO do we stop? We already issued diagnostics.
                }
            }
        }

        // At this point if we aren't at the root or direct child of the root, then the template is
        // incomplete. So it hasn't gotten to the ends of all logical sections.
        if (TemplateComplete(currentNode))
        {
            AddTemplateIncomplete(diagnostics);
        }

        if (diagnostics.Any())
        {
            return new(diagnostics);
        }
        else
        {
            var td = new TemplateDefinition(root, identifiers.ToImmutable());

            return new(td);
        }

        static void AddTemplateIncomplete(List<Diagnostic> diagnostics) =>
            diagnostics.Add(
                Diagnostic.Create(
                    Diagnostics.UnfinishedTemplate("Template is incomplete"),
                    null));

        static void AddExpectedClosingToken(List<Diagnostic> diagnostics, string actualText) =>
            diagnostics.Add(
                Diagnostic.Create(
                    Diagnostics.UnexpectedToken(actualText, "Expected closing '}}' token."),
                    null));

        static void AddMissingToken(List<Diagnostic> diagnostics, string missingToken) =>
            diagnostics.Add(
                Diagnostic.Create(
                    Diagnostics.MissingToken(missingToken),
                    null));
    }

    public static string RenderTemplate(
            TemplateDefinition template,
            ImmutableDictionary<string, RenderData> data)
    {
        var sb = new StringBuilder();
        data = data.WithComparers(StringComparer.OrdinalIgnoreCase); // Render without being case sensitive to identifiers.

        Render(sb, template.Syntax, data);

        return sb.ToString();

        static void Render(StringBuilder sb, TemplateSyntax syntax, ImmutableDictionary<string, RenderData> data)
        {
            if (syntax.Type == TemplateSyntaxType.Literal)
            {
                sb.Append(syntax.LiteralText);
                return;
            }

            if (data.TryGetValue(syntax.Identifier, out var renderData))
            {
                if (syntax.Type == TemplateSyntaxType.Identifier)
                {
                    var formatted = syntax.Format switch
                    {
                        FormatSetting.None => renderData.RenderString,
                        FormatSetting.Pascal => ToPascal(renderData.RenderString),
                        FormatSetting.Camel => ToCamel(renderData.RenderString),
                        _ => throw new InvalidOperationException($"Unknown Format Setting: {syntax.Format}"),
                    };

                    sb.Append(formatted);
                    return;
                }
                else if (syntax.Type == TemplateSyntaxType.If && renderData.Render == false)
                {
                    return;
                }
                else if (syntax.Type == TemplateSyntaxType.Not && renderData.Render == true)
                {
                    return;
                }
            }
            else if (syntax.Type == TemplateSyntaxType.If)
            {
                return; // We didn't find the identifier, so don't render the IF section.
            }

            foreach (var child in syntax.Children)
            {
                Render(sb, child, data);
            }

            static string ToPascal(string text)
            {
                if (char.IsUpper(text[0]))
                {
                    return text;
                }

                return char.ToUpperInvariant(text[0]) +
                    (text.Length > 1 ? text.Substring(1) : string.Empty);
            }

            static string ToCamel(string text)
            {
                if (char.IsLower(text[0]))
                {
                    return text;
                }

                return char.ToLowerInvariant(text[0]) +
                    (text.Length > 1 ? text.Substring(1) : string.Empty);
            }
        }
    }

    private static bool IdentifierIsFormatToken(string identifier, out FormatSetting f)
    {
        f = FormatSetting.None;

        if ("pascal".Equals(identifier, StringComparison.OrdinalIgnoreCase))
        {
            f = FormatSetting.Pascal;
            return true;
        }
        else if ("camel".Equals(identifier, StringComparison.OrdinalIgnoreCase))
        {
            f = FormatSetting.Camel;
            return true;
        }

        return false;
    }

    private static bool TokenKindsMatch(ReadOnlySpan<IndexedToken> tokens, params TokenType[] types)
    {
        if (tokens.IsEmpty)
        {
            return false; // We are out of tokens
        }

        if (types.Any() == false)
        {
            throw new InvalidOperationException("There must be types if there are tokens.");
        }

        if (tokens[0].Token.TokenType == types[0])
        {
            if (types.Length == 1)
            {
                return true; // Were done.
            }

            return TokenKindsMatch(tokens.Slice(1), types.Skip(1).ToArray());
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Find the next instance of a <paramref name="type"/>, if any, between here and the first <paramref name="stopAt"/> token.
    /// </summary>
    private static bool TokenKindSeek(ReadOnlySpan<IndexedToken> tokens, TokenType type, TokenType stopAt, out IndexedToken found)
    {
        found = default(IndexedToken);

        if (tokens.IsEmpty)
        {
            return false; // We are out of tokens
        }

        foreach (var t in tokens)
        {
            if(t.Token.TokenType == type)
            {
                found = t;
                return true;
            }
            else if(t.Token.TokenType == stopAt)
            {
                return false;
            }
        }

        return false;
    }

    private static bool TemplateComplete(TemplateSyntax? currentNode)
    {
        if (currentNode?.Type == TemplateSyntaxType.Root)
        {
            return false;
        }
        else if (currentNode?.Parent?.Type != TemplateSyntaxType.Root &&
            currentNode?.Type is TemplateSyntaxType.Identifier or TemplateSyntaxType.Literal)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if the identifier makes sense. For example '{{^id}}{{id}}{{/id}}' doesn't make sense
    /// because we are trying to use id where it doesn't exist.
    /// </summary>
    private static IdentifierValidity IdentiferValidInThisContext(TemplateSyntaxType newSyntaxType, TemplateSyntax? parent, string identifier)
    {
        if (parent is null)
        {
            return IdentifierValidity.Valid;
        }

        if (parent.Identifier.Equals(identifier, StringComparison.Ordinal))
        {
            if (newSyntaxType is TemplateSyntaxType.Identifier or TemplateSyntaxType.If && parent.Type is TemplateSyntaxType.Not)
            {
                return IdentifierValidity.IdentifierUsedWhereItDoesntExist;
            }
            else if (newSyntaxType is TemplateSyntaxType.Not && parent.Type is TemplateSyntaxType.Identifier or TemplateSyntaxType.If)
            {
                return IdentifierValidity.IdentifierExcludedWhereItDoesExist;
            }
        }

        return IdentiferValidInThisContext(newSyntaxType, parent.Parent, identifier);
    }

    /// <summary>
    /// Look forward for the next non white space after <paramref name="i"/>.
    /// </summary>
    /// <param name="tokens">Token collection</param>
    /// <param name="i">Starting index</param>
    /// <returns>Index and token of the next token, or null if none exists.</returns>
    private static ReadOnlySpan<IndexedToken> PeekNonWhitespace(
        ReadOnlySpan<Token> tokens,
        int i,
        List<IndexedToken>? nonWhitespaceTokens = null)
    {
        //TODO remove allocations in this method and return.
        nonWhitespaceTokens ??= new();

        if (i + 1 < tokens.Length)
        {
            i++;
            if (tokens[i].TokenType != TokenType.Whitespace)
            {
                nonWhitespaceTokens.Add(new(i, tokens[i]));
            }

            return PeekNonWhitespace(tokens, i, nonWhitespaceTokens);
        }
        else
        {
            return new ReadOnlySpan<IndexedToken>(nonWhitespaceTokens.ToArray());
        }
    }

    [DebuggerDisplay("{Start}-{End} {TokenType}")]
    public readonly struct Token
    {
        public Token(int start, int end, TokenType tokenType)
        {
            Start = start;
            End = end;
            TokenType = tokenType;
        }

        public int Start { get; }

        public int End { get; }

        public TokenType TokenType { get; }
    }

    [DebuggerDisplay("Index {Index}: {Token.Start}-{Token.End} {Token.TokenType}")]
    public readonly struct IndexedToken
    {
        public IndexedToken(int index, Token token)
        {
            Index = index;
            Token = token;
        }

        public int Index { get; }

        public Token Token { get; }
    }

    public readonly struct RenderData
    {
        public RenderData(string identity, string renderString, bool render)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            RenderString = renderString ?? throw new ArgumentNullException(nameof(renderString));
            Render = render;
        }

        public string Identity { get; }

        public string RenderString { get; }

        public bool Render { get; }

        public static implicit operator KeyValuePair<string, RenderData>(RenderData rd)
        {
            return new(rd.Identity, rd);
        }
    }

    public class TemplateDefinition
    {
        public TemplateDefinition(TemplateSyntax syntax, ImmutableHashSet<string> identifiers)
        {
            Syntax = syntax ?? throw new ArgumentNullException(nameof(syntax));
            Identifiers = identifiers ?? throw new ArgumentNullException(nameof(identifiers));
        }

        public TemplateSyntax Syntax { get; }

        public ImmutableHashSet<string> Identifiers { get; }
    }

    [DebuggerDisplay("{Type}: Id {Identifier} Text {LiteralText}")]
    public class TemplateSyntax
    {
        public TemplateSyntax(
            TemplateSyntax? parent,
            TemplateSyntaxType type,
            string literalText,
            string identifier,
            FormatSetting format)
        {
            Parent = parent;
            Type = type;
            LiteralText = literalText ?? throw new ArgumentNullException(nameof(literalText));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            Format = format;
        }

        public TemplateSyntaxType Type { get; }

        public string LiteralText { get; }

        public string Identifier { get; }

        public FormatSetting Format { get; }

        public TemplateSyntax? Parent { get; }

        public List<TemplateSyntax> Children { get; } = new();

        /// <summary>
        /// Counts the nodes of a particular type, starting from this node and checking all children recursively.
        /// </summary>
        /// <param name="selector">Criteria to count</param>
        /// <returns>Number of nodes.</returns>
        public int CountNodes(Func<TemplateSyntax, bool> selector)
        {
            int count = 0;

            if (selector(this))
            {
                count++;
            }

            foreach (var child in Children)
            {
                count += child.CountNodes(selector);
            }

            return count;
        }
    }
}
