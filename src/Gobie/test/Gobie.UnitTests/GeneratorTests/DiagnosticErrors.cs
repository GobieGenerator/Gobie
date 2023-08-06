﻿namespace Gobie.UnitTests.GeneratorTests;

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
    public Task GB1007_GlobalGenWrongTemplateItem()
    {
        var source = @"
        using Gobie;

        public sealed class GlobalExampleGenerator : Gobie.GobieClassGenerator
        {
            [GobieGlobalFileTemplate(""EFCoreRegistrationGenerator"")]
            private const string Globe = @""{{WRONG}}"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1007_GlobalGenMultipleChildContent()
    {
        var source = @"
        using Gobie;

        public sealed class GlobalExampleGenerator : Gobie.GobieClassGenerator
        {
            [GobieGlobalFileTemplate(""EFCoreRegistrationGenerator"")]
            private const string Globe = @""{{ChildContent}}{{ChildContent}}"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1011_MissingClosedTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName .AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test(Description = "We could error this as an invalid tag, but because we run into an Open tag after an Open, its probably more clear that the Close tag is missing.")]
    public Task GB1011_MissingClosedTag_NotGB1024()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName a s b {{.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1020_InvalidFormatToken()
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
    public Task GB1021_InterpolatedConstTemplate()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = $@""Just using the $ is disallowed"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1021_InterpolatedConstMultipleTemplates()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection1 = $@""Just using the $ is disallowed"";

            [GobieFileTemplate(""StateLog"")]
            private const string EncapsulatedCollection2 = $@""Just using the $ is disallowed"";

            [GobieGlobalChildTemplate(""EFCoreRegistrationGenerator"")]
            private const string EncapsulatedCollection3 = $@""Just using the $ is disallowed"";

            [GobieGlobalFileTemplate(""EFCoreRegistrationGenerator"", ""SomethingElse"")]
            private const string EncapsulatedCollection3 = $@""Just using the $ is disallowed"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1022_ConcatenatedConstTemplate()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection2 =
                ""interpolated const"" +
                ""{{EncapsulatedCollection : pascal}} "";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1022_ConcatenatedMultipleTemplates()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection1 =
                ""interpolated const"" +
                ""{{EncapsulatedCollection : pascal}} "";

            [GobieFileTemplate(""StateLog"")]
            private const string EncapsulatedCollection2 =
                ""interpolated const"" +
                ""{{EncapsulatedCollection : pascal}} "";

            [GobieGlobalChildTemplate(""EFCoreRegistrationGenerator"")]
            private const string EncapsulatedCollection3 =
                ""interpolated const"" +
                ""{{EncapsulatedCollection : pascal}} "";

            [GobieGlobalFileTemplate(""EFCoreRegistrationGenerator"", ""SomethingElse"")]
            private const string EncapsulatedCollection3 =
                ""interpolated const"" +
                ""{{EncapsulatedCollection : pascal}} "";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1023_EmptyTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{ }} => {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1024_InvalidTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{ ImUne'xpected }} => {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1024_InvalidTag2()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{ ImUne'xpected  => }} {{FieldName}}.AsReadOnly();"";
        }";

        return TestHelper.Verify(source);
    }

    [Test]
    public Task GB1025_UnexpectedTag()
    {
        var source = @"
        public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
        {
            [GobieTemplate]
            private const string EncapsulatedCollection = ""public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{ ImExpected }}{{ ImUnexpected }} => {{FieldName}}.AsReadOnly();  {{ ImUnexpected }} {{ MeToo }}"";

            public string ImExpected { get; set; }
        }";

        return TestHelper.Verify(source);
    }
}