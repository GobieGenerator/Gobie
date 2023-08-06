namespace Gobie.UnitTests.GeneratorTests;

/// <summary>
/// This class contains tests verifying true negatives of Gobie errors. For example, we verify that
/// our rule preventing partials for gobie generators does NOT error on other uses of partial.
/// </summary>
public class DiagnosticErrorsTrueNegatives
{
    [Test]
    public Task NoBase_Partial_NoDiagnostic()
    {
        var source = @"public partial class UserDefinedGenerator { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task NonGobie_Partial_NoDiagnostic()
    {
        var source = @"public partial class UserDefinedGenerator : FooBase { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1005_Unsealed_ButNonGobie()
    {
        var source = @"public class UserDefinedGenerator : FooBase { }";

        return TestHelper.Verify(source);
    }
}
