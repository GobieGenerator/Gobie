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

    // Note, we use the integers to ensure verify generates unique file names when escaping these strings.
    [TestCase(101, "private readonly ;")]
    [TestCase(102, "private readonly List<string names = new();")]
    [TestCase(103, "private readonly Liststring> names = new();")]
    [TestCase(104, "private readonly List<string> = new();")]
    [TestCase(105, "private readonly List<string> names new();")]
    [TestCase(106, "private readonly List<string> names;")]
    [TestCase(107, "private readonly List<> names;")]
    [TestCase(108, "private readonly List< names;")]
    [TestCase(109, "private readonly List> names;")]
    [TestCase(110, "private;")]
    [TestCase(201, "private readonly ;")]
    [TestCase(202, "private readonly List<string names = new()")]
    [TestCase(203, "private readonly Liststring> names = new()")]
    [TestCase(204, "private readonly List<string> = new()")]
    [TestCase(205, "private readonly List<string> names new()")]
    [TestCase(206, "private readonly List<string> names")]
    [TestCase(207, "private readonly List<> names")]
    [TestCase(208, "private readonly List< names")]
    [TestCase(209, "private readonly List> names")]
    [TestCase(210, "private;")]
    [TestCase(301, ";")]
    [TestCase(302, "")]
    public Task InvalidFieldDeclaration_DoesntCrash(int n, string field)
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        public sealed class EncapsulatedFieldGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string ReadonlyCollection = ""// {{FieldName}} {{FieldType}} {{FieldGenericType}}"";
        }

        public partial class TemplateTarget
        {
            [EncapsulatedField]
            " + field + @"
        }";

        return TestHelper.Verify(source);
    }
}
