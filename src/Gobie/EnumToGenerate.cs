namespace Gobie
{
    using System.Collections.Generic;

    public readonly struct EnumToGenerate
    {
        public readonly string Name;
        public readonly List<string> Values;

        public EnumToGenerate(string name, List<string> values)
        {
            Name = name;
            Values = values;
        }
    }
}
