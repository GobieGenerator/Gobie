namespace Gobie.Models;

public static class Config
{
    static Config()
    {
        var dict = ImmutableDictionary.CreateBuilder<string, string>();
        dict.Add("GobieClassGenerator", "global::Gobie.GobieClassGeneratorAttribute");
        dict.Add("Gobie.GobieClassGenerator", "global::Gobie.GobieClassGeneratorAttribute");
        dict.Add("GobieFieldGenerator", "global::Gobie.GobieFieldGeneratorAttribute");
        dict.Add("Gobie.GobieFieldGenerator", "global::Gobie.GobieFieldGeneratorAttribute");
        dict.Add("GobieGlobalGenerator", "global::Gobie.GobieAssemblyGeneratorAttribute");
        dict.Add("Gobie.GobieGlobalGenerator", "global::Gobie.GobieAssemblyGeneratorAttribute");
        GenToAttribute = dict.ToImmutable();
    }

    public static ImmutableDictionary<string, string> GenToAttribute { get; }
}
