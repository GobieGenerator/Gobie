namespace Gobie.UnitTests.GeneratorTests;

/// <summary>
/// This class tests all errors. Errors have error severity, but also should NOT be associated with
/// any generated output. Tests here are verifying particular line numbers. Both lines and columns
/// are zero indexed.
/// </summary>
public class ErrorDiagnostics
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
