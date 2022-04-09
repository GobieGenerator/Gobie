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

        [GobieGeneratorName(""PkGen"")]
        public sealed class PrimaryKeyGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""public int Id { get; set; } // This is a key"";
        }

        public sealed class NamePropertyGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""public string Name { get; set; }"";
        }

        [PkGenAttribute]
        [NamePropertyAttribute]
        public partial class GenTarget
        { }

        [PkGen]
        [NameProperty]
        public partial class GenTarget2
        { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_WithParameterUsage_GeneratesOutput()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        public sealed class NamePropertyGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""public string RobotName { get; set; } = \""{{InitialName}}{{^InitialName}}Nameless{{/InitialName}}-{{Id}}{{^Id}}Numberless{{/Id}}\"";"";

            [Required(1)]
            public string InitialName { get; set; }

            [Required(2)]
            public int Id { get; set; }
        }

        [NameProperty(""Mike"", 521351)]
        public partial class TemplateTarget
        { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_WithDefaultParameterAndUsage_GeneratesOutput()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        public sealed class NamePropertyGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""public string RobotName { get; set; } = \""{{InitialName}}{{^InitialName}}Nameless{{/InitialName}}-{{Id}}{{^Id}}Numberless{{/Id}}\"";"";

            [Required(1)]
            public string InitialName { get; set; }

            [Required(2)]
            public int Id { get; set; } = 2048234;
        }

        [NameProperty(""Mike"")]
        public partial class TemplateTarget
        { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_WithNamedParameterUsage_GeneratesOutput()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        public sealed class NamePropertyGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""public string RobotName { get; set; } = \""{{InitialName : camel}}{{^InitialName}}Nameless{{/InitialName}}-{{Id}}{{^Id}}Numberless{{/Id}}\"";"";

            [Required(1)]
            public string InitialName { get; set; }

            public int Id { get; set; }
        }

        [NameProperty(initialName: ""Mike"", Id = 2048234)]
        public partial class TemplateTarget
        { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SimpleValidGenerator_WithNullParameterUsage_GeneratesOutput()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        public sealed class NamePropertyGenerator : GobieFieldGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""public string RobotName { get; set; } = \""{{InitialName}}{{^InitialName}}Nameless{{/InitialName}}-{{Id}}{{^Id}}Numberless{{/Id}}\"";"";

            [Required(1)]
            public string InitialName { get; set; }

            public string Id { get; set; }
        }

        [NameProperty(null, Id = null)]
        public partial class TemplateTarget
        { }";

        return TestHelper.Verify(source);
    }
}
