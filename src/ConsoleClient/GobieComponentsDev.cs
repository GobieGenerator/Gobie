namespace Gobie
{
    using System;

    /// <summary>
    /// Base class Gobie ALWAYS generates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class GobieTemplateAttribute : Attribute
    {
    }

    public abstract class GobieGeneratorBase
    {
        public virtual bool DebugGenerator { get; protected set; }
    }

    /// <summary>
    /// Base class Gobie ALWAYS generates.
    /// </summary>
    public abstract class GobieFieldGenerator : GobieGeneratorBase
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public abstract class GobieFieldGeneratorAttribute : Attribute
    { }

    /// <summary>
    /// Attribute produced by gobie. Defined in <see cref="EncapulatedCollectionGenerator"/>.
    /// </summary>
    public sealed class EncapulatedCollectionAttribute : GobieFieldGeneratorAttribute
    {
        public string CustomValidator { get; set; } = null;
    }
}
