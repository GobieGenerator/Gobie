namespace Gobie
{
    using System;

    /// <summary>
    /// Base class Gobie ALWAYS generates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public abstract class GobieBaseFieldAttribute : Attribute
    {
    }

    /// <summary>
    /// Base class Gobie ALWAYS generates.
    /// </summary>
    public sealed class GobieTemplateAttribute : GobieBaseFieldAttribute
    {
    }

    /// <summary>
    /// Base class Gobie ALWAYS generates.
    /// </summary>
    public sealed class GobieFileTemplateAttribute : GobieBaseFieldAttribute
    {
        /// <summary>
        /// A literal string or mustache template to determine the file name. 
        /// </summary>
        public string FileNameTemplate { get; set; }
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
