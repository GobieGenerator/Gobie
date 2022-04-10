namespace Gobie.UnitTests;

using static Gobie.Models.Templating.Mustache;

[TestFixtureSource(nameof(FixtureArgs))]
public class MoustacheTokenizePropertyBasedTests
{
    private static object[] FixtureArgs = {
        new object[] { "Question"},
        new object[] { "{{Answer}}" },
        new object[] { "{{#name}}\r\n  <b>{{name}}</b>\r\n{{/name}}\r\n{{^name}}\r\n  No name. \r\n{{/name}}" },
        new object[] { "  {{name. fjkd}}</b>\r\n{{" },
    };

    private readonly string template;
    private readonly Token[] tokens;

    public MoustacheTokenizePropertyBasedTests(string template)
    {
        this.template = template;
        tokens = Tokenize(this.template).ToArray();
    }

    [Test]
    public void TokenNumberDoesntExceedTemplateLength() => Assert.IsTrue(template.Length >= tokens.Length);

    [Test]
    public void TokensArentNone() => Assert.AreEqual(0, tokens.Count(x => x.TokenType == TokenType.None));

    [Test]
    public void TokensMinPositionIsZero() => Assert.AreEqual(0, tokens.Min(x => x.Start));

    [Test]
    public void TokensMaxPositionIsTemplateLength() => Assert.AreEqual(template.Length - 1, tokens.Max(x => x.End));

    [Test]
    public void TokensHavePositiveLength() => Assert.AreEqual(0, tokens.Count(x => x.End < x.Start));

    [Test]
    public void TokensAreOrderedAndAdjacent()
    {
        for (int i = 0; i < tokens.Length - 1; i++)
        {
            Assert.AreEqual(tokens[i].End + 1, tokens[i + 1].Start);
        }
    }
}
