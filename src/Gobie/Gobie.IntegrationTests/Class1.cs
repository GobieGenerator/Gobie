namespace Gobie.IntegrationTests;

using Gobie;
using NUnit.Framework;

public sealed class EncapsulatedCollectionGenerator : GobieFieldGenerator
{
    [GobieTemplate]
    public const string Temp = """
        // Some comment
        public IEnumerable<{{FieldGenericType}}>  {{FieldName : pascal }} => {{FieldName}}.AsReadOnly();

         public void Add{{ FieldName : pascal }}({{FieldGenericType}} s)
        {
            if(s is null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            {{#CustomValidator}}
            if({{CustomValidator}}(s))
            {
                {{FieldName}}.Add(s);
            }
            {{/CustomValidator}}

            {{^CustomValidator}}
                {{FieldName}}.Add(s);
            {{/CustomValidator}}
        }
        """;

    public string CustomValidator { get; set; } = null!;
}

public partial class Encapsulation
{
    [EncapsulatedCollection]
    private List<string> authors = new();
}

public class Class1
{
    [Test]
    public void Test()
    {
    }
}
