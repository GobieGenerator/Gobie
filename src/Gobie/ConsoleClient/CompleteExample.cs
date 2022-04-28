namespace SomeNamespace
{
    using Gobie;
    using System.Collections.Generic;

    [GobieGeneratorName("EFCoreRegistrationAttribute", Namespace = "ConsoleClient")]
    public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
    {
        [GobieGlobalFileTemplate("EFCoreRegistrationGenerator", "EFCoreRegistration")]
        private const string KeyString = @"
            namespace SomeNamespace;

            public sealed static class EFCoreRegistration
            {
                public static void Register()
                {
                    {{#ChildContent}}
                        // Global generator code
                        {{ ChildContent}}
                    {{/ ChildContent}}
                    {{^ChildContent}}
                        // No generators reference this global generator.
                    {{/ ChildContent}}
                }
            }
            ";
    }

    public sealed class EncapsulatedCollectionGenerator : GobieFieldGenerator
    {
        [GobieTemplate]
        private const string ReadonlyCollection = "public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : pascal}} => {{FieldName}}.AsReadOnly(); // Encapsulating {{FieldType}}";

        [GobieGlobalChildTemplate("EFCoreRegistrationGenerator")]
        private const string EfCoreRegistration = "// Hello From {{ClassName}}.{{FieldName}}";
    }

    public partial class TemplateTarget
    {
        [EncapsulatedCollection]
        private readonly List<string> names = new(), addresses = new(), books = new();
    }

    public partial class OtherTarget
    {
        [EncapsulatedCollection]
        private readonly List<string> names = new();
    }
}
