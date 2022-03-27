namespace Gobie
{
    public static class SourceGenerationHelper
    {
        public const string GobieCore = @"
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

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public abstract class GobieFieldGeneratorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GobieGeneratorNameAttribute : Attribute
    {
        public GobieGeneratorNameAttribute(string attributeName)
        {
            AttributeName = attributeName ?? throw new ArgumentNullException(nameof(attributeName));
        }

        public string AttributeName { get; }
        public string Namespace { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class Required : Attribute
    {
        public Required(int order = int.MaxValue)
        {
            Order = order;
        }

        public int Order { get; }
    }
}";
    }
}
