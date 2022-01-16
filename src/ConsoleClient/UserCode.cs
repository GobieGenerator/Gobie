using Gobie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
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
}
