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

        public partial class PartialTest
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Partial_NoDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial class PartialTest : FooBase
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Unsealed_NoDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public class PartialTest : FooBase
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Partial_GetsDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial sealed class PartialTest : GobieFieldGenerator
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Unsealed_GetsDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public class PartialTest : GobieFieldGenerator
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task PartialUnsealed_GetsDiagnostics()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial class PartialTest : Gobie.GobieFieldGenerator
        {
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task Valid_NoDiagnostic()
    {
        var source = @"
        using NetEscapades.EnumGenerators;

        public class PartialTest
        {
        }";

        return TestHelper.Verify(source);
    }
}
