namespace Gobie.UnitTests.GeneratorTests;

/// <summary>
/// This class is for testing instances where there are c# syntax errors. So Roslyn is breaking the
/// build and we don't need to. But we need to not crash Gobie.
/// </summary>
internal class SyntaxErrors
{
    [Test]
    public Task GlobalFileTemplateAttribute_MissingArg_DoesntThrow()
    {
        var source = @"
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate()]
                private const string KeyString = @""
                namespace SomeNamespace;

                public static class EFCoreRegistration
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
    public Task GlobalFileTemplateAttribute_TooManyArgs_DoesntThrow()
    {
        var source = @"
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate(""EFCoreRegistration"", ""EXTRA ARG"")]
                private const string KeyString = @""
                namespace SomeNamespace;

                public static class EFCoreRegistration
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
    public Task GlobalFileTemplateAttribute_WrongArgType_DoesntThrow()
    {
        var source = @"
        [assembly: EFCoreRegistration]
        namespace SomeNamespace
        {
            using Gobie;
            public sealed class EFCoreRegistrationGenerator : GobieGlobalGenerator
            {
                [GobieGlobalFileTemplate(42)]
                private const string KeyString = @""
                namespace SomeNamespace;

                public static class EFCoreRegistration
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
