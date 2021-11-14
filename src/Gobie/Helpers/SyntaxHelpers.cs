using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobie.Helpers
{
    public static class SyntaxHelpers
    {
        public static FieldDeclarationSyntax? FindField(SyntaxNode node)
        {
            if (node is FieldDeclarationSyntax field)
            {
                return field;
            }
            if (node is SyntaxNode)
            {
                return FindField(node.Parent);
            }
            return null;
        }

        public static ClassDeclarationSyntax? FindClass(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax classDeclaration)
            {
                return classDeclaration;
            }
            if (node is SyntaxNode)
            {
                return FindClass(node.Parent);
            }
            return null;
        }
    }
}
