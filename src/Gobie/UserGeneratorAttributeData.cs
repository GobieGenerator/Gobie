﻿namespace Gobie;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

/// <summary>
/// Contains enough data to build the user generator attribute. But not enough to actually run the template.
/// </summary>
public class UserGeneratorAttributeData
{
    private readonly List<RequiredParameter> requiredParameters = new List<RequiredParameter>();

    public UserGeneratorAttributeData(string identifier, ClassDeclarationSyntax classDeclarationSyntax)
    {
        ClassDeclarationSyntax = classDeclarationSyntax;
        DefinitionIdentifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
    }

    public string NamespaceName { get; private set; } = "Gobie";

    public string DefinitionIdentifier { get; private set; }

    public string AttributeIdentifier =>
        DefinitionIdentifier + (DefinitionIdentifier.EndsWith("Attribute", StringComparison.Ordinal) ? string.Empty : "Attribute");

    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }

    public List<string> OptionalParameters { get; private set; } = new List<string>();

    public IEnumerable<RequiredParameter> RequiredParameters =>
        requiredParameters.OrderBy(x => x.RequestedOrder).ThenBy(x => x.DeclaredOrder);

    public void AddRequiredParameter(RequiredParameter p) => requiredParameters.Add(p);

    public UserGeneratorAttributeData WithName(string identifier, string? namespaceName)
    {
        DefinitionIdentifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

        if (!string.IsNullOrWhiteSpace(namespaceName))
        {
            NamespaceName = namespaceName!;
        }

        return this;
    }
}

/// <summary>
/// Class that contains all the data about the generator required to run the generatorion.
/// </summary>
public class UserGeneratorTemplateData
{ }

public class RequiredParameter
{
    public RequiredParameter(int requestedOrder, int declaredOrder, string name, string csharpTypeName)
    {
        name = name ?? throw new ArgumentNullException(nameof(name));
        RequestedOrder = requestedOrder;
        DeclaredOrder = declaredOrder;
        CsharpTypeName = csharpTypeName ?? throw new ArgumentNullException(nameof(csharpTypeName));

        NamePascal = name[0].ToString().ToUpperInvariant() + name.Substring(1);
        NameCamel = name[0].ToString().ToLowerInvariant() + name.Substring(1);
    }

    public int RequestedOrder { get; }

    public int DeclaredOrder { get; }

    public string NamePascal { get; }

    public string NameCamel { get; }

    public string CsharpTypeName { get; }

    public string PropertyString => $"public {CsharpTypeName} {NamePascal} {{ get; set; }}";

    public string CtorArgumentString => $"{CsharpTypeName} {NameCamel}";

    public string CtorAssignmentString => $"this.{NamePascal} = {NameCamel};";
}
