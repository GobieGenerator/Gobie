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
            namespace {{ClassNamespace}};

            public sealed static class EFCoreRegistration
            {
                public static Register()
                {
                    {{ChildContent}}
                }
            }
            "";
        }";

        return TestHelper.Verify(source);
    }
}
