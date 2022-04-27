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
            [GobieGlobalFileTemplate(""Log"", ""EFCoreRegistration"")]
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
    public Task SimpleValidGenerator_IdentifierNotChildContent_RaisesDiagnostic()
    {
        var source = @"
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate(""Log"", ""EFCoreRegistration"")]
                private const string KeyString = @""
                namespace SomeNamespace;

                public sealed static class EFCoreRegistration
                {
                    public static void Register()
                    {
                        {{OtherContent}}
                    }
                }
                "";
            }
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_MultipleChildContent_RaisesDiagnostic()
    {
        var source = @"
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate(""Log"", ""EFCoreRegistration"")]
                private const string KeyString = @""
                namespace SomeNamespace;

                public sealed static class EFCoreRegistration
                {
                    public static void Register()
                    {
                        {{ChildContent}}
                        {{ChildContent}}
                    }
                }
                "";
            }
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
                [GobieGlobalFileTemplate(""Log"", ""EFCoreRegistration"")]
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
}
