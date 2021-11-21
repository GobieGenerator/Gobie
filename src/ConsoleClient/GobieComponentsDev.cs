namespace Gobie
{
    using System;

    public abstract class GobieAssemblyGeneratorBaseAttribute : Attribute
    {
        public bool TemplateDebug { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public abstract class GobieAssemblyGeneratorAttribute : GobieAssemblyGeneratorBaseAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public abstract class GobieFieldGeneratorAttribute : GobieAssemblyGeneratorBaseAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class GobieTemplateAttribute : Attribute
    {
    }
}
