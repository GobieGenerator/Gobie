namespace Gobie.UnitTests.GeneratorTests;

/// <summary>
/// This class tests all errors. Errors have error severity, but also should NOT be associated with
/// any generated output. Therefore each should output a single json file through veriy. Tests here
/// are verifying particular line numbers. Both lines and columns are zero indexed.
/// </summary>
public class DiagnosticErrors
{
    [Test]
    public Task GB1004_UserGeneratorIsUnsealed()
    {
        var source = @"public sealed partial class UserDefinedGenerator : Gobie.GobieClassGenerator { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1004_GB1005_UserGeneratorIsPartialAndUnsealed()
    {
        var source = @"public partial class UserDefinedGenerator : Gobie.GobieClassGenerator { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1005_UserGeneratorIsNotSealed()
    {
        var source = @"public class UserDefinedGenerator : GobieClassGenerator { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1006_NameDoesntEndInGenerator()
    {
        var source = @"public sealed class UserDefined : Gobie.GobieClassGenerator { }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1020_InvalidFormatToken()
    {
        var source = @"
        public class foo {}

        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = @""
                    public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();
            "";
        }";

        return TestHelper.Verify(source);
    }
}
