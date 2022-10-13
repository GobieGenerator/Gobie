namespace Gobie.UnitTests.GeneratorTests;

public class DiagnosticErrorsCorrectLocInTemplates
{
    [Test]
    public Task BaselineExample()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task BaselineVerbatimExample()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = @""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task BaselineNewLine()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection =
                ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task BaselineVerbatimNewLine()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection =
                @""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task BadTagWithLeadingEscapedChar()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> \r\n \n{{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task BadTagStringWithLineBreaks()
    {
        // This isn't even recognized as containing a template. This has multiple string literal
        // expressions one node down, below the 'add' expression.

        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection =
                   ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}>"" +
                   ""{{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }
}
