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

        internal static bool ClassInheritsFrom(Compilation compilation, ClassDeclarationSyntax c, string targetName)
        {
            var sm = compilation.GetSemanticModel(c.SyntaxTree);
            //TODO this doesn't seem ideal.
            var typeInfo = (ITypeSymbol)sm.GetDeclaredSymbol(c);

            return SearchForType(typeInfo);

            bool SearchForType(ITypeSymbol? typeInfo)
            {
                if (typeInfo == null)
                {
                    return false;
                }
                if (typeInfo.Name == targetName)
                {
                    return true;
                }
                else if (typeInfo.BaseType is ITypeSymbol type)
                {
                    return SearchForType(type);
                }

                return false;
            }
        }

        internal static string GetClassname(Compilation compilation, ClassDeclarationSyntax c)
        {
            var sm = compilation.GetSemanticModel(c.SyntaxTree);
            //TODO this doesn't seem ideal.
            var typeInfo = (ITypeSymbol)sm.GetDeclaredSymbol(c);

            return typeInfo.Name;
        }
    }
}
