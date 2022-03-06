using NetEscapades.EnumGenerators.Tests;

namespace Gobie.UnitTests;

[TestFixture]
public class UserTemplateTests
{
    [Test]
    public Task NoBase_Partial_NoDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial class UserDefinedGenerator
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Partial_NoDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial class UserDefinedGenerator : FooBase
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Unsealed_NoDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public class UserDefinedGenerator : FooBase
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Partial_GetsDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial sealed class UserDefinedGenerator : GobieFieldGenerator
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Unsealed_GetsDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public class UserDefinedGenerator : GobieFieldGenerator
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task PartialUnsealed_GetsDiagnostics()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial class UserDefinedGenerator : Gobie.GobieFieldGenerator
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Empty_GetsNoDefinitionDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public sealed class UserDefinedGenerator : Gobie.GobieFieldGenerator
        {
        }";

        return TestHelper.Verify(source);
    }
}
