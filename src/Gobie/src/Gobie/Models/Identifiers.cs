namespace Gobie.Models;

/// <summary>
/// These are identifiers that Gobie defines, and cannot be redefined by users, even outside the scope they are available in.
/// </summary>
public static class Identifiers
{


    public const string ClassNameIdentifier = "ClassName";
    public const string ClassNamespaceIdentifier = "ClassNamespace";
    public const string FieldGenericTypeIdentifier = "FieldGenericType";
    public const string FieldNameIdentifier = "FieldName";
    public const string FieldTypeIdentifier = "FieldType";

    static Identifiers()
    {
        var hs = ImmutableHashSet.CreateBuilder<string>();
        hs.Add(ClassNameIdentifier);
        hs.Add(ClassNamespaceIdentifier);
        hs.Add(FieldGenericTypeIdentifier);
        hs.Add(FieldNameIdentifier);
        hs.Add(FieldTypeIdentifier);
        IdentifierTokens = hs.ToImmutable();

    }

    public static ImmutableHashSet<string> IdentifierTokens { get; }
}
