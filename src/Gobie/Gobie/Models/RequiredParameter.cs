namespace Gobie.Models;

using Microsoft.CodeAnalysis;
using System;

public class RequiredParameter
{
    //TODO support required parameters having default values.
    public RequiredParameter(int requestedOrder, Location location, int declaredOrder, string name, string csharpTypeName, string initalizerLiteral)
    {
        InitalizerLiteral = initalizerLiteral;
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#144-constants
        // The type specified in a constant declaration shall be sbyte, byte, short, ushort, int,
        // uint, long, ulong, char, float, double, decimal, bool, string, an enum_type, or a reference_type.

        name = name ?? throw new ArgumentNullException(nameof(name));

        RequestedOrder = requestedOrder;
        RequestedOrderLocation = location;
        DeclaredOrder = declaredOrder;
        CsharpTypeName = csharpTypeName ?? throw new ArgumentNullException(nameof(csharpTypeName));

        NamePascal = name[0].ToString().ToUpperInvariant() + name.Substring(1);
        NameCamel = name[0].ToString().ToLowerInvariant() + name.Substring(1);
    }

    public int RequestedOrder { get; }

    public Location RequestedOrderLocation { get; }

    public int DeclaredOrder { get; }

    public string NamePascal { get; }

    public string NameCamel { get; }

    public string CsharpTypeName { get; }

    public string InitalizerLiteral { get; set; }

    public string Initalizer =>
        string.IsNullOrWhiteSpace(InitalizerLiteral) ? string.Empty : $" = {InitalizerLiteral}";

    public string PropertyString => $"public {CsharpTypeName} {NamePascal} {{ get; }}";

    public string CtorArgumentString => $"{CsharpTypeName} {NameCamel}{Initalizer}";

    public string CtorAssignmentString => $"this.{NamePascal} = {NameCamel};";
}
