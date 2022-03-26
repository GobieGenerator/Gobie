namespace Gobie;

using Gobie.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        TemplateTokenOpen,

        /// <summary>
        /// This token is comprised of char that can be an identifier.
        /// </summary>
        Identifier,

        Whitespace,
        Period
    }

    public enum TemplateSyntaxType
    {
        Root,
        Literal,
        If,
        Not,
        Identifier,
    }

    private enum IdentifierValidity
    {
        Valid,
        IdentiferUsedWhereItDoesntExist,
        IdentiferExcludedWhereItDoesExist,
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
            else if (template[i] == '.')
            {
                AddOrCombine(tokens, new Token(i, i, TokenType.Period));
            }
            else if (char.IsWhiteSpace(template[i]))
            {
                AddOrCombine(tokens, new Token(i, i, TokenType.Whitespace));
            }
            else if (char.IsNumber(template[i])
                     || char.IsLetter(template[i])
                     || template[i] == '_')
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

    public static DataOrDiagnostics<TemplateDefinition> Parse(ReadOnlySpan<char> template)
    {
        var tokens = Tokenize(template);
        var root = new TemplateSyntax(null, TemplateSyntaxType.Root);
        var diagnostics = new List<Diagnostic>();

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

            bool TokenIsText(ReadOnlySpan<Token> tokens, int i) => tokens[i].TokenType is TokenType.General or TokenType.Identifier or TokenType.Whitespace;
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

                currentNode.Children.Add(new TemplateSyntax(currentNode, TemplateSyntaxType.Literal, sb.ToString()));
            }
            else if (tokens[i].TokenType is TokenType.Close)
            {
                diagnostics.Add(
                    Diagnostic.Create(
                        Errors.UnexpectedToken(
                            GetText(template, tokens[i]),
                            "There is no corresponding open token"),
                        null));
            }
            else
            {
                // Here, the only remaining types are opening tokens.
                var tagClosed = false;
                var identiferFound = false;
                var identifier = string.Empty;
                var initialToken = tokens[i];
                bool continueSeeking = false;

                if (TrySeekNonWhitespace(tokens, ref i, out var t))
                {
                    if (t.TokenType == TokenType.Identifier)
                    {
                        // This is our only good case.
                        continueSeeking = true;
                        identiferFound = true;
                        identifier = GetText(template, t);
                    }
                    else
                    {
                        diagnostics.Add(
                            Diagnostic.Create(
                                Errors.UnexpectedToken(
                                    GetText(template, t),
                                    "Expected an identifier string, which contains only letters, numbers, and underscores"),
                                null));
                    }
                }
                else
                {
                    diagnostics.Add(
                          Diagnostic.Create(
                              Errors.UnfinishedTemplate(
                                  "Template is incomplete"),
                              null));
                }

                if (continueSeeking)
                {
                    if (TrySeekNonWhitespace(tokens, ref i, out var t2))
                    {
                        if (t2.TokenType == TokenType.Close)
                        {
                            // This is our only good case. The tag was closed
                            tagClosed = true;
                        }
                        else
                        {
                            diagnostics.Add(
                                Diagnostic.Create(
                                    Errors.UnexpectedToken(
                                        GetText(template, t2),
                                        "Expected closing '}}' token. Note, identifiers cannot have white space."),
                                    null));
                        }
                    }
                    else
                    {
                        diagnostics.Add(
                              Diagnostic.Create(
                                  Errors.UnfinishedTemplate(
                                      "Template is incomplete"),
                                  null));
                    }
                }

                if (identiferFound && tagClosed && initialToken.TokenType != TokenType.LogicEndOpen)
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
                    if (identValid == IdentifierValidity.IdentiferUsedWhereItDoesntExist)
                    {
                        diagnostics.Add(
                            Diagnostic.Create(
                                Errors.UnreachableTemplateSection(
                                    $"You cannot use the identifer '{identifier}' here because it is surrounded " +
                                    $"by a not node (i.e. {{{{^{identifier}}}}} )"),
                                null));
                    }
                    else if (identValid == IdentifierValidity.IdentiferExcludedWhereItDoesExist)
                    {
                        diagnostics.Add(
                           Diagnostic.Create(
                              Errors.UnreachableTemplateSection(
                                  $"You cannot exclude the identifer '{identifier}' here because it is surrounded " +
                                  $"by an if node (i.e. {{{{#{identifier}}}}} )"),
                              null));
                    }

                    // We close the syntax even if identifier isn't valid, b/c we don't want
                    // inaccurate alerts saying the template is incomplete.
                    var ts = new TemplateSyntax(currentNode, tst, string.Empty, identifier);
                    currentNode.Children.Add(ts);
                    currentNode = ts.Type == TemplateSyntaxType.Identifier ? currentNode : ts;
                }
                if (identiferFound && tagClosed && initialToken.TokenType == TokenType.LogicEndOpen)
                {
                    // Here we found a complete valid tag which ends a logical tag. We need to check
                    // if the currentNode is the matching opening tag (the good case) and move the
                    // current node back to be its parent. If not we need to report diagnostics.

                    if (currentNode.Type is TemplateSyntaxType.If or TemplateSyntaxType.Not)
                    {
                        if (currentNode.Identifier.Equals(identifier, StringComparison.Ordinal))
                        {
                            // The node matches, so we close it, by navigating back to the parent
                            currentNode = currentNode.Parent;
                        }
                        else
                        {
                            // The logical end doesn't match, so we issue a diagnostic.
                            if (currentNode.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                            {
                                diagnostics.Add(
                                    Diagnostic.Create(
                                        Errors.UnexpectedIdentifier(
                                            identifier,
                                            $"The provided identifier differs only in case from '{currentNode.Identifier}'. Identifier matching is case sensitive."),
                                        null));
                            }
                            else
                            {
                                // The node doesn't match at all, so show what we expect.
                                diagnostics.Add(
                                   Diagnostic.Create(
                                       Errors.LogicalEndMissing("{{\\" + currentNode.Identifier + "}}"),
                                       null));
                            }
                        }
                    }
                    else
                    {
                        // Here, we aren't able to close a node. So we issue a diagnostic.
                        diagnostics.Add(
                            Diagnostic.Create(
                                Errors.UnexpectedToken(
                                    GetText(template, tokens[i]),
                                    "There is no corresponding if or not tag (i.e. {{#name}} or {{^name}} for this to close"),
                                null));
                    }
                }
                else
                {
                    // TODO do we stop? We already issued dagnostics.
                }
            }
        }

        // At this point if we aren't at the root or direct child of the root, then the template is
        // incomplete. So it hasn't gotten to the ends of all logical sections.
        if (currentNode?.Type != TemplateSyntaxType.Root && currentNode?.Parent?.Type != TemplateSyntaxType.Root)
        {
            diagnostics.Add(
              Diagnostic.Create(
                  Errors.UnfinishedTemplate(
                      "Template is incomplete"),
                  null));
        }

        if (diagnostics.Any())
        {
            return new(diagnostics);
        }
        else
        {
            var td = new TemplateDefinition(root, ImmutableHashSet.Create<string>());

            return new(td);
        }
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
                return IdentifierValidity.IdentiferUsedWhereItDoesntExist;
            }
            else if (newSyntaxType is TemplateSyntaxType.Not && parent.Type is TemplateSyntaxType.Identifier or TemplateSyntaxType.If)
            {
                return IdentifierValidity.IdentiferExcludedWhereItDoesExist;
            }
        }

        return IdentiferValidInThisContext(newSyntaxType, parent.Parent, identifier);
    }

    /// <summary>
    /// Seeks the next non whitespace, if one exists. Advances <paramref name="i"/> as needed to
    /// move past white space.
    /// </summary>
    /// <param name="tokens"></param>
    /// <param name="i"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static bool TrySeekNonWhitespace(ReadOnlySpan<Token> tokens, ref int i, out Token token)
    {
        token = default;
        if (i + 1 < tokens.Length)
        {
            i++;
            if (tokens[i].TokenType != TokenType.Whitespace)
            {
                token = tokens[i];
                return true;
            }

            return TrySeekNonWhitespace(tokens, ref i, out token);
        }
        else
        {
            return false;
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

    public class TemplateSyntax
    {
        public TemplateSyntax(TemplateSyntax? parent, TemplateSyntaxType type, string literalText = "", string identifier = "")
        {
            Parent = parent;
            Type = type;
            LiteralText = literalText ?? throw new ArgumentNullException(nameof(literalText));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public TemplateSyntaxType Type { get; set; }

        public string LiteralText { get; set; } = string.Empty;

        public string Identifier { get; set; } = string.Empty;

        public TemplateSyntax? Parent { get; set; }

        public List<TemplateSyntax> Children { get; set; } = new();
    }
}
