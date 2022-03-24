namespace NetEscapades.EnumGenerators.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Enable();

        VerifierSettings.UseStrictJson(); // Multiline templates will be a mess otherwise
        VerifierSettings.AddExtraSettings(x =>
        {
            x.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Populate;
        });
    }
}
