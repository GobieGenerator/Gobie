namespace Gobie.Helpers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public static  class ConstantTypes
{
    public static bool IsAllowedConstantType(TypeSyntax syntax)
    {
        if (syntax is not PredefinedTypeSyntax s)
            return false;

        return s.Keyword.ValueText switch
        {
            "int" => true,
            "string" => true,
            _ => false,
        };
    }
}
