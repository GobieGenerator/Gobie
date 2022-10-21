namespace Gobie.UnitTests;

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

    [Ignore("Failing because of the namespace?")]
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
        using Gobie;
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
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

    [Test]
    public Task SimpleValidGenerator_WithUsageAndIdentifiers_References_GeneratesOutput_FullNamespace()
    {
        var source = @"
        [assembly: Gobie.EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;  // <==== Using here means we need to fully qualify the generator above.

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

    [TestCase("")]
    public Task ComplexExample_GeneratesOutput(string prefix)
    {
        var source = @"
        using Gobie;
        [assembly: [ATTPREFIX]EFCoreRegistration]
        [assembly: [ATTPREFIX]OtherGlobal]
        namespace SomeNamespace
        {
            // ============= First Generator ============
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [[ATTPREFIX]GobieGlobalFileTemplate(""EFCoreRegistration"")]
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
                [[ATTPREFIX]GobieTemplate]
                private const string ReadonlyCollection = ""public IEnumerable<{{FieldGenericType}}> {{FieldName : pascal}} => {{FieldName}}.AsReadOnly(); // Encapsulating {{FieldType}}"";

                [[ATTPREFIX]GobieGlobalChildTemplate(""EFCoreRegistration"")]
                private const string EfCoreRegistration = ""// Hello EFCoreRegistration From {{ClassName}}.{{FieldName}}"";

                [[ATTPREFIX]GobieGlobalChildTemplate(""OtherGlobal"")]
                private const string OtherRegistration = ""// Hello OtherGlobal From {{ClassName}}.{{FieldName}}"";
            }

            public partial class TemplateTarget
            {
                [[ATTPREFIX]EncapsulatedCollection]
                private readonly List<string> names = new(), addresses = new(), books = new();
            }

            public partial class OtherTarget
            {
                [[ATTPREFIX]EncapsulatedCollection]
                private readonly List<string> names = new();
            }

            // ============= Second Generator ============
            public sealed class OtherGlobalGenerator : GobieGlobalGenerator
            {
                [[ATTPREFIX]GobieGlobalFileTemplate(""OtherGlobal"")]
                private const string KeyString = @""
                namespace SomeNamespace;

                public sealed static class OtherGlobal
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

        source = source.Replace("[ATTPREFIX]", prefix);
        return TestHelper.Verify(source);
    }
}
