using Gobie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    public class EncapulatedCollectionAttribute : GobieFieldGeneratorAttribute
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

        public string CustomValidator { get; set; } = null;
    }
}
