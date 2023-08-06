namespace Gobie.UnitTests;

[TestFixture]
public class FileGen
{
    [Test]
    public Task SimpleValidGenerator_WithUsage_GeneratesOutput()
    {
        var source = @"
        using Gobie;

        namespace SomeNamespace;

        [GobieGeneratorName(""LoggedClass"")]
        public sealed class PrimaryKeyGenerator : GobieClassGenerator
        {
            [GobieTemplate]
            private const string KeyString = ""private readonly List<{{ClassName}}Log> logs = new();"";

            [GobieFileTemplate(""Log"")]
            private const string KeyString = @""
            namespace {{ClassNamespace}};

            public sealed class {{ClassName}}Log
            {
                public int Id { get; set; }

                public {{ClassName}} Parent {get; set;}

                public DateTime Timestamp {get; set;}

                public string LogMessage {get; set;}
            }
            "";
        }

        [LoggedClassAttribute]
        public partial class GenTarget
        { }";

        return TestHelper.Verify(source);
    }
}
