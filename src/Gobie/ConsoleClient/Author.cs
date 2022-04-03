using Gobie;
using System.Collections.Generic;

namespace ConsoleClient.Models
{
    public sealed class EncapsulatedCollectionGenerator : GobieFieldGenerator
    {
        [GobieTemplate]
        private const string EncapsulationTemplate =
@"
            private readonly List<{{TypeName}}> {{Field}} = new List<{{TypeName}}>();
            public IEnumerable<{{TypeName}}> {{PropertyName}} => {{Field}}.AsReadOnly();
            public IEnumerable<int> {{ Property }}Lengths => {{ Field }}.Select(x => x.Length);
        ";

        [GobieTemplate]
        private const string AddMethod =
    @"      public void Add{{ Property }}({{TypeName}} s)
            {
                {{#CustomValidator}}
                if({{CustomValidator}}(s))
                {
                    {{Field}}.Add(s);
                }
                {{/CustomValidator}}

                {{^CustomValidator}}
                    {{Field}}.Add(s);
                {{/CustomValidator}}
            }";

        [Required]
        public string PropertyName { get; set; }

        [Required]
        public string Field { get; set; }

        [Required]
        public string TypeName { get; set; }

        [Required]
        public string CustomValidator { get; set; } = null;
    }

    [EncapsulatedCollection("Books", "books", "string", nameof(ValidateBooks))]
    public partial class GenTarget
    {
        public bool ValidateBooks(string s) => s.Length > 0;
    }
}
