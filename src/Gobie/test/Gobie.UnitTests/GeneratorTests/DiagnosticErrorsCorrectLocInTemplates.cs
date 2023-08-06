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
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> \r\n \n{{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();\n\n"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task BadTagWithLeadingEscapedSlashes()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> \\\\\\ \\{{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();\n\n"";
        }";

        return TestHelper.Verify(source);
    }

    [Ignore("For some reason this combination of delimiters is causing this to fail, but removing them individually from the template resolves it when testing in program.cs")]
    [Test]
    public Task BadTagWithAllLeadingEscapedChar()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> \'\""\\\0\a\b\f\n\r\t\v{{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();\n\n"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task BadTagWithLeadingTabs()
    {
        // Tabs, unsurprisingly, are counted as single characters. So no special handling was needed
        // to correctly issue the diagnostic.
        var source =
        "public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator \r\n" +
        "{ \r\n" +
        "    [GobieTemplate]\r\n" +
        "\t\tprivate const string EncapsulatedCollection = \"public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();\";\r\n" +
        "}";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task MultiLineRawStringLiteral_BadTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = """"""
                 public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();
"""""";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task MultiLineRawStringLiteral_Indenting_BadTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = """"""
                 public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();
                 """""";  // <-Removes leading spaces from string contents
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task MultiLineRawStringLiteral_IndentingMultipleLines_BadTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = """"""
                 //// Some comment text....
                 //// Even More text...
                 public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();
                 """""";  // <-Removes leading spaces from string contents
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task MultiLineRawStringLiteral_ExtraQuotes_BadTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = """"""""""
                 public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();
                 """""""""";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SingleLineRawStringLiteral_BadTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = """"""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();"""""";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task SingleLineRawStringLiteral_ExtraQuotes_BadTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = """"""""""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();"""""""""";
        }";

        return TestHelper.Verify(source);
    }
}
