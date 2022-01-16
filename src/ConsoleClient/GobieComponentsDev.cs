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
    /// User provided generator definition.
    /// </summary>
    public class EncapulatedCollectionGenerator : GobieFieldGenerator
    {
        [GobieTemplate]
        private const string EncapsulationTemplate =
@"      public IEnumerable<string> {{ Property }} => {{ field }}.AsReadOnly();

        public IEnumerable<int> {{ Property }}Lengths => {{ field }}.Select(x => x.Length);
";

        [GobieTemplate]
        private const string AddMethod =
@"      public bool TryAdd{{ Property }}(string s)
        {
            {{#CustomValidator}}
            if({{CustomValidator}}(s))
            {
                {{ field }}.Add(s);
                return true;
            }
            return false;
            {{/CustomValidator}}
            {{^CustomValidator}}
            {{ field }}.Add(s);
            return true;
            {{/CustomValidator}}
        }";

        public override bool DebugGenerator { get; protected set; } = true;

        public string CustomValidator { get; set; } = null;
    }

    /// <summary>
    /// Attribute produced by gobie. Defined in <see cref="EncapulatedCollectionGenerator"/>.
    /// </summary>
    public sealed class EncapulatedCollectionAttribute : GobieFieldGeneratorAttribute
    {
        public string CustomValidator { get; set; } = null;
    }
}
