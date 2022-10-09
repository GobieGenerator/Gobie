namespace Gobie.UnitTests.GeneratorTests;

/// <summary>
/// Tests here are verifying particular line numbers. Both lines and columns are zero indexed.
/// </summary>
public class DiagnosticTests
{
    [Test]
    public Task GB1005_UserGeneratorIsNotSealed()
    {
        var source = @"
        public class UserDefinedGenerator : GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulationTemplate = "";
        }";

        return TestHelper.Verify(source);
    }
}
