namespace Gobie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

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
