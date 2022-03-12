namespace Gobie;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

/// <summary>
/// Contains enough data to build the user generator attribute. But not enough to actually run the template.
/// </summary>
public class UserGeneratorAttributeData
{
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
