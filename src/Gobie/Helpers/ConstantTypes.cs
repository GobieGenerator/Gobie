namespace Gobie.Helpers;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public static  class ConstantTypes
{
    public static bool IsAllowedConstantType(TypeSyntax syntax, out string typeName)
    {
        typeName = string.Empty;

        if (syntax is not PredefinedTypeSyntax s)
            return false;

        typeName = s.Keyword.ValueText;
        return s.Keyword.ValueText switch
        {
            "int" => true,
            "string" => true,
            _ => false,
        };
    }
}
