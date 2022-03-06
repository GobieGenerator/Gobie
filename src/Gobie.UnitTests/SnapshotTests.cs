using NetEscapades.EnumGenerators.Tests;

namespace Gobie.UnitTests;

[TestFixture]
public class SnapshotTests
{
    [Test]
    public Task GeneratesEnumExtensionsCorrectly()
    {
        // The source code to test
        var source = @"
using NetEscapades.EnumGenerators;

[EnumExtensions]
public enum Colour
{
Red = 0,
Blue = 1,
Green = 2,
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Test]
    public Task NoBase_Partial_NoDiagnostic()
    {
        // The source code to test
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial class PartialTest
        {
        }";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Partial_NoDiagnostic()
    {
        // The source code to test
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial class PartialTest : FooBase
        {
        }";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Unsealed_NoDiagnostic()
    {
        // The source code to test
        var source = @"
        using NetEscapades.EnumGenerators;

        public class PartialTest : FooBase
        {
        }";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Test]
    public Task Partial_GetsDiagnostic()
    {
        // The source code to test
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial sealed class PartialTest : GobieBaseSomething
        {
        }";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Test]
    public Task Unsealed_GetsDiagnostic()
    {
        // The source code to test
        var source = @"
        using NetEscapades.EnumGenerators;

        public class PartialTest : GobieBaseSomething
        {
        }";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Test]
    public Task PartialUnsealed_GetsDiagnostics()
    {
        // The source code to test
        var source = @"
        using NetEscapades.EnumGenerators;

        public partial class PartialTest : GobieBaseSomething
        {
        }";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Test]
    public Task Valid_NoDiagnostic()
    {
        // The source code to test
        var source = @"
        using NetEscapades.EnumGenerators;

        public class PartialTest
        {
        }";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}
