using Gobie;

namespace Models
{
    public sealed class EncapsulatedCollectionGenerator : GobieClassGenerator
    {
        [GobieTemplate]
        private const string EncapsulationTemplate =
@"
            private readonly System.Collections.Generic.List<{{TypeName}}> {{PropertyName : camel}} = new System.Collections.Generic.List<{{TypeName}}>();
            public System.Collections.Generic.IEnumerable<{{TypeName}}> {{PropertyName : pascal}} => {{PropertyName : camel}}.AsReadOnly();
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
