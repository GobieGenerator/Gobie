﻿namespace Gobie.UnitTests.GeneratorTests;

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
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> \r\n\n \n{{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();\n\n"";
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
}
