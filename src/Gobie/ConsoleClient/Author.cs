using Gobie;
using System.Collections.Generic;

namespace Models
{
    public sealed class EncapsulatedCollectionGenerator : GobieFieldGenerator
    {
        [GobieTemplate]
        private const string EncapsulationTemplate =
        @"
            public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : pascal}} => {{FieldName : camel}}.AsReadOnly();
        ";

        [GobieTemplate]
        private const string AddMethod =
        @"
        public void Add{{ FieldName : pascal }}({{FieldGenericType}} s)
            {
                {{#CustomValidator}}
                if({{CustomValidator}}(s))
                {
                    {{FieldName : camel}}.Add(s);
                }
                {{/CustomValidator}}

                {{^CustomValidator}}
                    {{FieldName : camel}}.Add(s);
                {{/CustomValidator}}
            }";

        [Required]
        public string CustomValidator { get; set; } = null;
    }

    [GobieGeneratorName("LoggedClass")]
    public sealed class LoggedClassGenBase : GobieClassGenerator
    {
        [GobieTemplate]
        private const string LogString = "private readonly System.Collections.Generic.List<{{ClassName}}Log> logs = new();";

        [GobieFileTemplate("Log")]
        private const string KeyString = @"
            using System;

            namespace {{ClassNamespace}};

            public sealed class {{ClassName}}Log
            {
                public int Id { get; set; }

                public {{ClassName}} Parent {get; set;}

                public DateTime Timestamp {get; set;}

                public string LogMessage {get; set;}
            }
            ";
    }

    [LoggedClass]
    public partial class Author
    {
        [EncapsulatedCollection(nameof(ValidateBooks))]
        private List<string> books = new();

        public bool ValidateBooks(string s) => s.Length > 0;
    }
}
