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
    public sealed class GobieGlobalChildTemplateAttribute : GobieBaseFieldAttribute
    {
        /// <summary>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public GobieGlobalChildTemplateAttribute(string templateName)
        {
            TemplateName = templateName ?? throw new ArgumentNullException(nameof(templateName));
        }

        public string TemplateName { get; }
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

    /// <summary>
    /// Base class Gobie ALWAYS generates.
    /// </summary>
    public abstract class GobieGlobalGenerator : GobieGeneratorBase
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

    [AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
    public abstract class GobieAssemblyGeneratorAttribute : Attribute
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

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class GobieGlobalFileTemplateAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        /// <param name="templateName">
        /// Name of the template, also used as the output name from the generator '.g.cs'
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public GobieGlobalFileTemplateAttribute(string templateName)
        {
            TemplateName = templateName ?? throw new ArgumentNullException(nameof(templateName));
        }

        public string TemplateName { get; }
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
