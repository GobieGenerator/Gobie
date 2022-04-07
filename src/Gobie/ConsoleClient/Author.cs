using Gobie;
using System.Collections.Generic;

namespace ConsoleClient.Models
{
    public sealed class EncapsulatedCollectionGenerator : GobieFieldGenerator
    {
        [GobieTemplate]
        private const string EncapsulationTemplate =
@"
            private readonly List<{{TypeName}}> {{PropertyName : camel}} = new List<{{TypeName}}>();
            public IEnumerable<{{TypeName}}> {{PropertyName : pascal}} => {{PropertyName : camel}}.AsReadOnly();
            public IEnumerable<int> {{ PropertyName : pascal }}Lengths => {{PropertyName : camel}}.Select(x => x.Length);
        ";

        [GobieTemplate]
        private const string AddMethod =
    @"      public void Add{{ PropertyName }}({{TypeName}} s)
            {
                {{#CustomValidator}}
                if({{CustomValidator}}(s))
                {
                    {{PropertyName : camel}}.Add(s);
                }
                {{/CustomValidator}}

                {{^CustomValidator}}
                    {{PropertyName : camel}}.Add(s);
                {{/CustomValidator}}
            }";

        [Required]
        public string PropertyName { get; set; }

        [Required]
        public string TypeName { get; set; }

        [Required]
        public string CustomValidator { get; set; } = null;
    }

    [EncapsulatedCollection("Books", "string", nameof(ValidateBooks))]
    public partial class GenTarget
    {
        public bool ValidateBooks(string s) => s.Length > 0;
    }
}
