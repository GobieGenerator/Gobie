namespace Gobie.UnitTests;

[TestFixture]
public class FieldGen
{
    [Test]
    public Task Generator_WithTemplateSyntaxIssue_SkipsOutputGeneration()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        [GobieGeneratorName(""PkGen"")]
        public sealed class PrimaryKeyGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""{{"";
        }

        [PkGenAttribute]
        public partial class GenTarget
        { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_WithUsage_GeneratesOutput()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        public sealed class EncapsulatedFieldGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string ReadonlyCollection = ""public {{FieldType}} {{FieldName : pascal}} => {{FieldName}}; // Readonly {{FieldType}}"";
        }

        public partial class TemplateTarget
        {
            [EncapsulatedField]
            private readonly string names = new();
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGeneratorGeneric_WithUsage_GeneratesOutput()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        public sealed class EncapsulatedCollectionGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string ReadonlyCollection = ""public IEnumerable<{{FieldGenericType}}> {{FieldName : pascal}} => {{FieldName}}.AsReadOnly(); // Encapsulating {{FieldType}}"";
        }

        public partial class TemplateTarget
        {
            [EncapsulatedCollection]
            private readonly List<string> names = new();
        }";

        return TestHelper.Verify(source);
    }
}
