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
@"
        public System.Collections.Generic.IEnumerable<string> {{ Property }} => {{ field }}.AsReadOnly();

        public System.Collections.Generic.IEnumerable<int> {{ Property }}Lengths => {{ field }}.Select(x => x.Length);
";

        [GobieTemplate]
        private const string AddMethod =
@"
        public void Add{{ Property }}(string s)
        {
            {{#validator}}
            if({{validator}}(s))
            {
                {{ field }}.Add(s);
            }
            {{/validator}}
            {{^validator}}
            {{ field }}.Add(s);
            {{/validator}}
        }
";

        private readonly string validator;

        public EncapulatedCollectionAttribute(string validator = "")
                                    : base(false)
        {
            this.validator = validator;
        }

        public string CustomValidator { get; set; } = null;

        public string CustomSideEffect { get; set; } = null;
    }
}
