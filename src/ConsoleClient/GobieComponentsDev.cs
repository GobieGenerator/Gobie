namespace Gobie
{
    public class GobieAssemblyGeneratorBaseAttribute : System.Attribute
    {
        public GobieAssemblyGeneratorBaseAttribute(bool debug)
        {
            Debug = debug;
        }

        public bool Debug { get; private set; }
    }

    [System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
    public class GobieAssemblyGeneratorAttribute : GobieAssemblyGeneratorBaseAttribute
    {
        public GobieAssemblyGeneratorAttribute(bool debug = true)
            : base(debug)
        {
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class GobieFieldGeneratorAttribute : GobieAssemblyGeneratorBaseAttribute
    {
        public GobieFieldGeneratorAttribute(string template, bool debug = true)
            : base(debug)
        {
            Template = template;
        }

        public string Template { get; }
    }
}
