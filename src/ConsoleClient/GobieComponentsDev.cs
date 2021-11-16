namespace Gobie
{
    using System;

    public abstract class GobieAssemblyGeneratorBaseAttribute : Attribute
    {
        public GobieAssemblyGeneratorBaseAttribute(bool debug)
        {
            Debug = debug;
        }

        public bool Debug { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public abstract class GobieAssemblyGeneratorAttribute : GobieAssemblyGeneratorBaseAttribute
    {
        public GobieAssemblyGeneratorAttribute(bool debug = true)
            : base(debug)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public abstract class GobieFieldGeneratorAttribute : GobieAssemblyGeneratorBaseAttribute
    {
        public GobieFieldGeneratorAttribute(bool debug = true)
            : base(debug)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class GobieTemplateAttribute : Attribute
    {
    }
}
