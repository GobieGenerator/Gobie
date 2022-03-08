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
    public void SimpleValidGenerator_AttributeGenerated()
    {
        Assert.Fail();
    }
}
