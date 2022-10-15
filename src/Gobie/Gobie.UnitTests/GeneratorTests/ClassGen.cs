namespace Gobie.UnitTests;

[TestFixture]
public class ClassGen
{
    private const string TrivialTemplate = @"
    {
        [GobieTemplate]
        private const string EncapsulationTemplate = "";// String Comment"";
    }";

    ////[Test]
    ////public Task Empty_GetsNoDefinitionDiagnostic()
    ////{
    ////    Assert.Fail("Not workign yet");
    ////    var source = @"
    ////    using Gobie;

    ////    public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
    ////    {
    ////    }";

    ////    return TestHelper.Verify(source);
    ////}

    [Test]
    public Task ValidNameOverridenByAttribute()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGenerator"")]
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NameSuppliedByAttribute()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGenerator"")]
        public sealed class UserDefined : Gobie.GobieClassGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NameEndingInAttributeSuppliedByAttribute()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGeneratorAttribute"")]
        public sealed class UserDefined : Gobie.GobieClassGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NameAndNamespaceSuppliedByAttribute()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGenerator"", Namespace = ""MyNamespace"")]
        public sealed class UserDefined : Gobie.GobieClassGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task MultipleNameAndNamespaceSuppliedByAttributes_UsesFirst()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGenerator"", Namespace = ""MyNamespace"")]
        [GobieGeneratorName(""SeconGen"", Namespace = ""SecondNamespace"")]
        public sealed class UserDefined : Gobie.GobieClassGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task PartialAttribute()
    {
        // Strangely this does work.
        var source = @"
        using Gobie;

        [GobieGeneratorName(""PartialName
        public sealed class UserDefined : Gobie.GobieClassGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Ignore("Known to be failing")]
    [Test]
    public Task AttributeWithoutArgs()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName
        public sealed class UserDefined : Gobie.GobieClassGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_WithOptionalParam()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            public string MyParam {get; set;}

            public string OtherParam {get; set;} = ""My Initial Value"";

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_WithOptionalAndRequiredParam()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required]
            public string ReqParam {get; set;}

            public string MyParam {get; set;}

            public string OtherParam {get; set;} = ""My Initial Value"";

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_WithOptionalAndTwoRequiredParam()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required]
            public string ReqParam {get; set;}

            [Required]
            public int ReqParamTheSecond {get; set;}

            public string MyParam {get; set;}

            public string OtherParam {get; set;} = ""My Initial Value"";

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParams_SpecificPriorityPrioritized()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required]
            public string ReqNoPositionRequest {get; set;}

            [Required(1)]
            public int ReqRequestedFirst {get; set;}

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParams_SpecificPrioritiesOrdered()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required(5)]
            public string ReqRequestedFifth {get; set;}

            [Required(1)]
            public int ReqRequestedFirst {get; set;}

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParamsTies_OutputDiagnostics()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required(5)]
            public string ReqRequestedFifth {get; set;}

            [Required(1)]
            public int ReqRequestedFirst {get; set;}

            [Required(5)]
            public int AlsoReqRequestedFifth {get; set;}

            [Required(5)]
            public int AnotherReqRequestedFifth {get; set;}

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParams_AllowedTypes()
    {
        var source = @"
        using Gobie;

        public class CustomType {}

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required]
            public int MyInt {get; set;}

            [Required]
            public string MyString {get; set;}

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParams_DisallowedType_GetsNonBlockingDiagnostic()
    {
        var source = @"
        using Gobie;

        public class CustomType {}

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required]
            public double BadProp {get; set;}

            [Required]
            public string MyString {get; set;}

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParams_WithInitalizersInOrder_Generates()
    {
        var source = @"
        using Gobie;

        public class CustomType {}

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required]
            public int MyInt {get; set;} = 4;

            [Required(1)]
            public string MyString {get; set;}

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParams_WithInitalizersOutOfOrder_NoGobieDiagnostic()
    {
        // This generates invalid constructor, but Roslyn will point out the issue. if we need to we
        // can add some hand holding.

        var source = @"
        using Gobie;

        public class CustomType {}

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            [Required]
            public int MyInt {get; set;} = 4;

            [Required]
            public string MyString {get; set;}

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_OptionalParams_DisallowedType_GetsNonBlockingDiagnostic()
    {
        var source = @"
        using Gobie;

        public class CustomType {}

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieClassGenerator
        {
            public double BadProp {get; set;}

            public string MyString {get; set;}

            [GobieTemplate]
            private const string EncapsulationTemplate = ""// String Comment"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_WithTemplateSyntaxIssue_SkipsOutputGeneration()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        [GobieGeneratorName(""PkGen"")]
        public sealed class PrimaryKeyGenerator : GobieClassGenerator
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
        public sealed class PrimaryKeyGenerator : GobieClassGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""public int Id { get; set; } // This is a key"";
        }

        public sealed class NamePropertyGenerator : GobieClassGenerator
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

        public sealed class NamePropertyGenerator : GobieClassGenerator
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

        public sealed class NamePropertyGenerator : GobieClassGenerator
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

        public sealed class NamePropertyGenerator : GobieClassGenerator
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

        public sealed class NamePropertyGenerator : GobieClassGenerator
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
