namespace Gobie.Models;
using System;

public class OptionalParameter
{
    public OptionalParameter(string name, string csharpTypeName, string initalizerLiteral)
    {
        InitalizerLiteral = initalizerLiteral;
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#144-constants
        // The type specified in a constant declaration shall be sbyte, byte, short, ushort, int,
        // uint, long, ulong, char, float, double, decimal, bool, string, an enum_type, or a reference_type.

        name = name ?? throw new ArgumentNullException(nameof(name));

        CsharpTypeName = csharpTypeName ?? throw new ArgumentNullException(nameof(csharpTypeName));

        NamePascal = name[0].ToString().ToUpperInvariant() + name.Substring(1);
    }

    public string NamePascal { get; }

    public string CsharpTypeName { get; }

    public string InitalizerLiteral { get; }

    public string Initalizer =>
        string.IsNullOrWhiteSpace(InitalizerLiteral) ? string.Empty : $" = {InitalizerLiteral};";

    public string PropertyString => $"public {CsharpTypeName} {NamePascal} {{ get; set; }}{Initalizer}";
}
