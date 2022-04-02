using Gobie.Tests;

namespace Gobie.UnitTests;

[TestFixture]
public class UserTemplateTests
{
    private const string TrivialTemplate = @"
    {
        [GobieTemplate]
        private const string EncapsulationTemplate = ""// String Comment"";
    }";

    [Test]
    public Task NoBase_Partial_NoDiagnostic()
    {
        var source = @"
        using Gobie;

        public partial class UserDefinedGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Partial_NoDiagnostic()
    {
        var source = @"
        using Gobie;

        public partial class UserDefinedGenerator : FooBase" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Unsealed_NoDiagnostic()
    {
        var source = @"
        using Gobie;

        public class UserDefinedGenerator : FooBase" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Partial_GetsDiagnostic()
    {
        var source = @"
        using Gobie;

        public partial sealed class UserDefinedGenerator : GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Unsealed_GetsDiagnostic()
    {
        var source = @"
        using Gobie;

        public class UserDefinedGenerator : GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task PartialUnsealed_GetsDiagnostics()
    {
        var source = @"
        using Gobie;

        public partial class UserDefinedGenerator : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Empty_GetsNoDefinitionDiagnostic()
    {
        Assert.Fail("Not workign yet");
        var source = @"
        using Gobie;

        public sealed class UserDefinedGenerator : Gobie.GobieFieldGenerator
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NameDoesntEndInGenerator_GetsDiagnostic()
    {
        var source = @"
        using Gobie;

        public sealed class UserDefined : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task ValidNameOverridenByAttribute()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGenerator"")]
        public sealed class UserDefinedGenerator : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NameSuppliedByAttribute()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGenerator"")]
        public sealed class UserDefined : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NameEndingInAttributeSuppliedByAttribute()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGeneratorAttribute"")]
        public sealed class UserDefined : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NameAndNamespaceSuppliedByAttribute()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGenerator"", Namespace = ""MyNamespace"")]
        public sealed class UserDefined : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task MultipleNameAndNamespaceSuppliedByAttributes_UsesFirst()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName(""MyGenerator"", Namespace = ""MyNamespace"")]
        [GobieGeneratorName(""SeconGen"", Namespace = ""SecondNamespace"")]
        public sealed class UserDefined : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task PartialAttribute()
    {
        // Strangely this does work.
        var source = @"
        using Gobie;

        [GobieGeneratorName(""PartialName
        public sealed class UserDefined : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task AttributeWithoutArgs()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName
        public sealed class UserDefined : Gobie.GobieFieldGenerator" + TrivialTemplate;

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_WithOptionalParam()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            public string MyParam {get; set;}

            public string OtherParam {get; set;} = ""My Initial Value"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_WithOptionalAndRequiredParam()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required]
            public string ReqParam {get; set;}

            public string MyParam {get; set;}

            public string OtherParam {get; set;} = ""My Initial Value"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_WithOptionalAndTwoRequiredParam()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required]
            public string ReqParam {get; set;}

            [Required]
            public int ReqParamTheSecond {get; set;}

            public string MyParam {get; set;}

            public string OtherParam {get; set;} = ""My Initial Value"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParams_SpecificPriorityPrioritized()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required]
            public string ReqNoPositionRequest {get; set;}

            [Required(1)]
            public int ReqRequestedFirst {get; set;}
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParams_SpecificPrioritiesOrdered()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required(5)]
            public string ReqRequestedFifth {get; set;}

            [Required(1)]
            public int ReqRequestedFirst {get; set;}
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Generator_ReqParamsTies_OutputDiagnostics()
    {
        var source = @"
        using Gobie;

        [GobieGeneratorName]
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required(5)]
            public string ReqRequestedFifth {get; set;}

            [Required(1)]
            public int ReqRequestedFirst {get; set;}

            [Required(5)]
            public int AlsoReqRequestedFifth {get; set;}

            [Required(5)]
            public int AnotherReqRequestedFifth {get; set;}
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
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required]
            public int MyInt {get; set;}

            [Required]
            public string MyString {get; set;}
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
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required]
            public double BadProp {get; set;}

            [Required]
            public string MyString {get; set;}
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
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required]
            public int MyInt {get; set;} = 4;

            [Required(1)]
            public string MyString {get; set;}
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
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            [Required]
            public int MyInt {get; set;} = 4;

            [Required]
            public string MyString {get; set;}
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
        public sealed class PrimaryKeyGenerator : Gobie.GobieFieldGenerator
        {
            public double BadProp {get; set;}

            public string MyString {get; set;}
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
            private const string KeyString = ""public string Name { get; set; } = \""{{InitialName}}\"";"";

            [Required]
            public string InitialName { get; set; }
        }

        [NameProperty(""Mike"")]
        public partial class Target
        { }";

        return TestHelper.Verify(source);
    }
}
