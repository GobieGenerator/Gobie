namespace Gobie.UnitTests;

public class MustacheTemplateSyntaxTests
{
    [TestCase(1, "")]
    [TestCase(2, "private void")]
    [TestCase(6, "{{#name}}A person named:{{name}}{{/name}}{{^name}}Nameless{{/name}}")]
    public void CountNodes_AllNodes_Succeeds(int expectedCount, string template)
    {
        // Keep in mind there is always a root node
        var parse = Mustache.Parse(template);
        Assert.AreEqual(expectedCount, parse.Data?.Syntax.CountNodes(x => true));
    }

    [TestCase(0, "")]
    [TestCase(0, "private void")]
    [TestCase(0, "{{name}}")]
    [TestCase(3, "{{#name}}A person named:{{name}}{{/name}}{{^name}}Nameless{{#alias}} with an alias:{{alias}}{{/alias}}{{/name}}")]
    public void CountNodes_LogicalNodes_Succeeds(int expectedCount, string template)
    {
        // Keep in mind there is always a root node
        var parse = Mustache.Parse(template);
        Assert.AreEqual(expectedCount, parse.Data?.Syntax.CountNodes(
            x => x.Type is Mustache.TemplateSyntaxType.If or Mustache.TemplateSyntaxType.Not));
    }
}
