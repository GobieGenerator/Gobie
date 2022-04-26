[assembly: EFCoreRegistrationGenerator]

namespace SomeNamespace
{
    using Gobie;

    public sealed class EFCoreRegistration : GobieGlobalGenerator
    {
        [GobieGlobalFileTemplate("Log", "EFCoreRegistration")]
        private const string KeyString = @"
            namespace SomeNamespace;

            public sealed static class EFCoreRegistration
            {
                public static void Register()
                {
                    // Initially no child content will be used.
                }
            }
            ";
    }
}
