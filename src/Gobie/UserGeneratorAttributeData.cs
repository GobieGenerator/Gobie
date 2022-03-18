namespace Gobie;

using Gobie.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

/// <summary>
/// Contains enough data to build the user generator attribute. But not enough to actually run the template.
/// </summary>
public class UserGeneratorAttributeData
{
    private readonly List<RequiredParameter> requiredParameters = new List<RequiredParameter>();

    public UserGeneratorAttributeData(ClassIdentifier defintionIdentifier, ClassDeclarationSyntax classDeclarationSyntax)
    {
        DefinitionIdentifier = defintionIdentifier;
        AttributeIdentifier = CalculateAttributeIdentifier(defintionIdentifier);
        ClassDeclarationSyntax = classDeclarationSyntax;
    }

    public ClassIdentifier DefinitionIdentifier { get; private set; }

    public ClassIdentifier AttributeIdentifier { get; private set; }

    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }

    public List<OptionalParameter> OptionalParameters { get; private set; } = new List<OptionalParameter>();

    public IEnumerable<RequiredParameter> RequiredParameters =>
        requiredParameters.OrderBy(x => x.RequestedOrder).ThenBy(x => x.DeclaredOrder);

    public void AddRequiredParameter(RequiredParameter p) => requiredParameters.Add(p);

    public UserGeneratorAttributeData WithName(string identifier, string? namespaceName)
    {
        namespaceName = string.IsNullOrWhiteSpace(namespaceName) ? AttributeIdentifier.NamespaceName : namespaceName;
        identifier += identifier.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase) ? "" : "Attribute";

        AttributeIdentifier = new ClassIdentifier(namespaceName!, identifier);

        return this;
    }

    private static ClassIdentifier CalculateAttributeIdentifier(ClassIdentifier defintionIdentifier)
    {
        const string Generator = "Generator";
        const string Attribute = "Attribute";

        var defName = defintionIdentifier.ClassName;
        var attName = defName;

        if (defName.EndsWith(Generator, StringComparison.OrdinalIgnoreCase))
        {
            attName = defName.Substring(0, defName.Length - Generator.Length);
        }
        attName += Attribute;

        return new ClassIdentifier(defintionIdentifier.NamespaceName, attName);
    }
}

/// <summary>
/// Class that contains all the data about the generator required to run the generatorion.
/// </summary>
public class UserGeneratorTemplateData
{
    public UserGeneratorTemplateData(UserGeneratorAttributeData data, List<string> templates)
    {
        AttributeData = data;
        Templates = templates;
    }

    public List<string> Templates { get; }

    public UserGeneratorAttributeData AttributeData { get; }
}

/// <summary>
/// Has a 1:1 mapping of templates and the targets they are mapped to.
/// </summary>
public class TargetAndTemplateData
{
    public TargetAndTemplateData(TemplateType templateType, string generatorName, ClassIdentifier targetClass, string code)
    {
        TemplateType = templateType;
        GeneratorName = generatorName ?? throw new ArgumentNullException(nameof(generatorName));
        TargetClass = targetClass ?? throw new ArgumentNullException(nameof(targetClass));
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    public TemplateType TemplateType { get; }

    public string GeneratorName { get; }

    public ClassIdentifier TargetClass { get; }

    public string Code { get; }
}

public class ClassIdentifier
{
    public ClassIdentifier(string classNamespace, string className)
    {
        NamespaceName = classNamespace ?? throw new ArgumentNullException(nameof(classNamespace));
        ClassName = className ?? throw new ArgumentNullException(nameof(className));
    }

    public string NamespaceName { get; }

    public string ClassName { get; }

    public string FullName =>
    $"{NamespaceName}{(string.IsNullOrWhiteSpace(NamespaceName) ? "" : ".")}{ClassName}";

    public string GlobalName =>
        $"global::{FullName}";
}

public class RequiredParameter
{
    public RequiredParameter(int requestedOrder, Location location, int declaredOrder, string name, string csharpTypeName)
    {
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

    public string PropertyString => $"public {CsharpTypeName} {NamePascal} {{ get; }}";

    public string CtorArgumentString => $"{CsharpTypeName} {NameCamel}";

    public string CtorAssignmentString => $"this.{NamePascal} = {NameCamel};";
}

public class OptionalParameter
{
    public OptionalParameter(string name, string csharpTypeName)
    {
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#144-constants
        // The type specified in a constant declaration shall be sbyte, byte, short, ushort, int,
        // uint, long, ulong, char, float, double, decimal, bool, string, an enum_type, or a reference_type.

        name = name ?? throw new ArgumentNullException(nameof(name));

        CsharpTypeName = csharpTypeName ?? throw new ArgumentNullException(nameof(csharpTypeName));

        NamePascal = name[0].ToString().ToUpperInvariant() + name.Substring(1);
    }


    public string NamePascal { get; }


    public string CsharpTypeName { get; }

    public string PropertyString => $"public {CsharpTypeName} {NamePascal} {{ get; set; }}";
}