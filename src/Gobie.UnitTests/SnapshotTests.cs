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
    public Task Partial_GetsDiagnostic()
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
    public Task NonPartial_NoDiagnostic()
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
