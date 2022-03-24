namespace Gobie.UnitTests;

public class MustacheTests
{
    [TestCase("{{")]
    [TestCase("{{/")]
    [TestCase("{{^")]
    [TestCase("{{#")]
    [TestCase("}}")]
    [TestCase("{{}}")]
    [TestCase("{{ a b }}")] // TODO should this have just one diagnostic or two?
    public Task Parse_InvalidValidFlatAst(string template) =>
    Verify(new ParseResult(template, Mustache.Parse(template)))
        .UseDirectory("Snapshots\\Mustache");

    [TestCase("")]
    [TestCase("just some text")]
    [TestCase("\n {{ name }}  ")]
    public Task Parse_ValidFlatAst(string template) =>
        Verify(new ParseResult(template, Mustache.Parse(template)))
            .UseDirectory("Snapshots\\Mustache");

    [TestCase("{{#name}}Something{{/name}} {{^name}}Something Else{{/name}}")]
    public Task Parse_WithLogicAst(string template) =>
        Verify(new ParseResult(template, Mustache.Parse(template)))
            .UseDirectory("Snapshots\\Mustache");

    private record ParseResult(string Template, DataOrDiagnostics<Mustache.TemplateSyntax> Result);
}
