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

    /// <summary>
    /// Base class Gobie ALWAYS generates.
    /// </summary>
    public abstract class GobieClassGenerator : GobieGeneratorBase
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public abstract class GobieFieldGeneratorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public abstract class GobieClassGeneratorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class GobieFileTemplateAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        /// <param name="fileName">Must be unique across all... File name is suffixed with '.g.cs'</param>
        /// <exception cref="ArgumentNullException"></exception>
        public GobieFileTemplateAttribute(string fileName)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        public string FileName { get; }
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
}
