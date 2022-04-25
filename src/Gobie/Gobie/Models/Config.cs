namespace Gobie.Models
{
    public static class Config
    {
        static Config()
        {
            var db = ImmutableDictionary.CreateBuilder<string, string>();
            db.Add("GobieClassGenerator", "global::Gobie.GobieClassGeneratorAttribute");
            db.Add("Gobie.GobieClassGenerator", "global::Gobie.GobieClassGeneratorAttribute");
            db.Add("GobieFieldGenerator", "global::Gobie.GobieFieldGeneratorAttribute");
            db.Add("Gobie.GobieFieldGenerator", "global::Gobie.GobieFieldGeneratorAttribute");
            db.Add("GobieGlobalGenerator", "global::Gobie.GobieGlobalTemplateChildAttribute");
            db.Add("Gobie.GobieGlobalGenerator", "global::Gobie.GobieGlobalTemplateChildAttribute");
            GenToAttribute = db.ToImmutable();
        }

        public static ImmutableDictionary<string, string> GenToAttribute { get; }
    }
}
