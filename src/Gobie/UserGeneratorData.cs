namespace Gobie
{
    using System;

    public class UserGeneratorData
    {
        public UserGeneratorData(string identifier)
        {
            DefinitionIdentifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public string NamespaceName { get; private set; } = "Gobie";

        public string DefinitionIdentifier { get; private set; }

        public string AttributeIdentifier =>
            DefinitionIdentifier + (DefinitionIdentifier.EndsWith("Attribute", StringComparison.Ordinal) ? string.Empty : "Attribute");

        public UserGeneratorData WithName(string identifier, string? namespaceName)
        {
            DefinitionIdentifier = identifier ?? throw new ArgumentNullException(nameof(identifier));

            if (!string.IsNullOrWhiteSpace(namespaceName))
            {
                NamespaceName = namespaceName;
            }

            return this;
        }
    }
}
