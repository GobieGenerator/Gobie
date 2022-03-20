namespace Gobie;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Mustache
{
    public static void Tokenize(ReadOnlySpan<char> template)
    {
        var tokens = new Span<Token>(new Token[template.Length]);

        int t = 0;

        for (int i = 0; i < template.Length; i++)
        {
            // Take advantage of language checking every one of these till it hits one.
            if (TryMatch(tokens, template, "{{#", TokenType.LogicIfOpen)
                || TryMatch(tokens, template, "{{^", TokenType.LogicNotOpen)
                || TryMatch(tokens, template, "{{/", TokenType.LogicEndOpen)
                || TryMatch(tokens, template, "}}", TokenType.Close))
            {
            }
            else if (template[i] == '.')
            {
                AddOrCombine(new Token(i, i, TokenType.Period));
            }
            else if (char.IsWhiteSpace(template[i]))
            {
                AddOrCombine(new Token(i, i, TokenType.Whitespace));
            }
            else if (char.IsNumber(template[i])
                     || char.IsLetter(template[i])
                     || template[i] == '_')
            {
                AddOrCombine(new Token(i, i, TokenType.General));
            }
            else
            {
                AddOrCombine(new Token(i, i, TokenType.General));
            }

            

            

            bool TryMatch(Span<Token> tokens, ReadOnlySpan<char> template, string compare, TokenType tokenType)
            {
                var len = compare.Length;
                if(i + len > template.Length)
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

                tokens[t] = new Token(i, i + len - 1, tokenType);
                t++;
                i += compare.Length - 1; // Advance, but keep in mind we are going to still get i++'
                return true;
            }
        }

        void AddOrCombine(Token token)
        {
            if (t == 0)
            {
                tokens[t] == token;
                t++;
            }

            if (token.TokenType == TokenType.None) continue;

            if (token.TokenType is TokenType.Whitespace or TokenType.General &&
                token.TokenType == tokens[t].TokenType)
            {
                tokens[t] = new Token(token.Start, token.End + 1, token.TokenType);
            }
            else
            {
                // Here we have a token we can't combine
                t++;
                tokens[t] = token;
            }
        }

        var final = tokens.Slice(0, t + 1);
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

    public enum TokenType
    {
        None = 0, 
        Open,
        Close,
        LogicIfOpen,
        LogicNotOpen,
        LogicEndOpen,
        General,

        /// <summary>
        /// This token is comprised of char that can be an identifier.
        /// </summary>
        Identifier,
        Whitespace,
        Period
    }
}
