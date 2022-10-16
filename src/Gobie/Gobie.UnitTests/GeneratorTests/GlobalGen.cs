﻿namespace Gobie.UnitTests;

[TestFixture]
public class GlobalGen
{
    [Test]
    public Task SimpleValidGenerator_CreatesFieldAttribute()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
        {
            [GobieGlobalFileTemplate(""EFCoreRegistration"")]
            private const string KeyString = @""
            namespace SomeNamespace;

            public sealed static class EFCoreRegistration
            {
                public static void Register()
                {
                    {{ChildContent}}
                }
            }
            "";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_WithUsageNoIdentifiers_GeneratesOutput()
    {
        var source = @"
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate(""EFCoreRegistration"")]
                private const string KeyString = @""
                namespace SomeNamespace;

                public sealed static class EFCoreRegistration
                {
                    public static void Register()
                    {
                        // Initially no child content will be used.
                    }
                }
                "";
            }
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_WithUsageAndIdentifiers_NoReferences_GeneratesOutput()
    {
        var source = @"
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate(""EFCoreRegistration"")]
                private const string KeyString = @""
                namespace SomeNamespace;

                public sealed static class EFCoreRegistration
                {
                    public static void Register()
                    {
                        {{#ChildContent}}
                            // Global generator code
                            {{ChildContent}}
                        {{/ChildContent}}
                        {{^ChildContent}}
                            // No generators reference this global generator.
                        {{/ChildContent}}
                    }
                }
                "";
            }
        }";

        return TestHelper.Verify(source);
    }

    [Ignore("Known to be failing")]
    [Test]
    public Task SimpleValidGenerator_WithUsageAndIdentifiers_AttrUsesNamespace_GeneratesOutput()
    {
        var source = @"
        [assembly: SomeNamespace.EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate(""EFCoreRegistration"")]
                private const string KeyString = @""
                namespace SomeNamespace;

                public sealed static class EFCoreRegistration
                {
                    public static void Register()
                    {
                        {{#ChildContent}}
                            // Global generator code
                            {{ChildContent}}
                        {{/ChildContent}}
                        {{^ChildContent}}
                            // No generators reference this global generator.
                        {{/ChildContent}}
                    }
                }
                "";
            }
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_WithUsageAndIdentifiers_References_GeneratesOutput()
    {
        var source = @"
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate(""EFCoreRegistration"")]
                private const string KeyString = @""
                namespace SomeNamespace;

                public sealed static class EFCoreRegistration
                {
                    public static void Register()
                    {
                        {{#ChildContent}}
                            // Global generator code
                            {{ChildContent}}
                        {{/ChildContent}}
                        {{^ChildContent}}
                            // No generators reference this global generator.
                        {{/ChildContent}}
                    }
                }
                "";
            }

            public sealed class EncapsulatedCollectionGenerator : GobieFieldGenerator
            {
                [GobieTemplate]
                private const string ReadonlyCollection = ""public IEnumerable<{{FieldGenericType}}> {{FieldName : pascal}} => {{FieldName}}.AsReadOnly(); // Encapsulating {{FieldType}}"";

                [GobieGlobalChildTemplate(""EFCoreRegistration"")]
                private const string EfCoreRegistration = ""// Hello From {{ClassName}}.{{FieldName}}"";
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
        }";

        return TestHelper.Verify(source);
    }
}
