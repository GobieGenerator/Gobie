namespace Gobie.IntegrationTests;

using Gobie;
using NUnit.Framework;

public class EncapsulatedGeneratorIntegrationTests
{
    [Test]
    public Task EncapsulatedCollectionValidatorWorks()
    {
        var lib = new Library();

        lib.AddBooks("A");
        lib.AddBooks("AB");
        lib.AddBooks("ABC");
        lib.AddBooks("ABCD");

        lib.AddAuthors("A"); // Omitted by custom validator
        lib.AddAuthors("AB");
        lib.AddAuthors("ABC"); // Omitted by custom validator
        lib.AddAuthors("ABCD");

        return Verify(lib).UseDirectory("Snapshots");
    }
}

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

            {{ asfd1blkj }}

            {{#customvalidator}}
            if({{CustomValidator}}(s))
            {
                {{fieldName}}.Add(s);
            }
            {{/CustomValidator}}

            {{^CustomValidator}}
                {{fieldName}}.Add(s);
            {{/CustomValidator}}
        }
        """;

    public string CustomValidator { get; set; } = null!;
}

public partial class Library
{
    [EncapsulatedCollection(CustomValidator = nameof(ValidateAuthor))]
    private List<string> authors = new();

    [EncapsulatedCollection]
    private List<string> books = new();

    private static bool ValidateAuthor(string author)
    {
        return author.Length % 2 == 0;
    }
}
