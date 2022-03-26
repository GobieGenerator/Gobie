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
    [TestCase("{{#name}}Someone named {{name}} {{#age}} with age of {{age}}{{/age}} {{/name}} is great!!")]
    [TestCase("{{#name}} Text1 {{#foo}}{{#name}}{{/name}} Text2  {{/foo}}{{/name}}")] // Inner if name is redudant.
    public Task Parse_WithLogicAst(string template) =>
        Verify(new ParseResult(template, Mustache.Parse(template)))
            .UseDirectory("Snapshots\\Mustache");

    [TestCase("{{^name}} Text1 {{name}} Text2 {{/name}}")]
    [TestCase("{{^name}} Text1 {{#name}}{{/name}} Text2 {{/name}}")]
    [TestCase("{{^name}} Text1 {{#foo}}{{name}} Text2  {{/foo}}{{/name}}")]
    [TestCase("{{^name}} Text1 {{#foo}}{{#name}}{{/name}} Text2  {{/foo}}{{/name}}")]
    [TestCase("{{#name}} Text1 {{#foo}}{{^name}}{{/name}} Text2  {{/foo}}{{/name}}")]
    public Task Parse_WithInvalidLogic_IssuesDiagnostics(string template) =>
    Verify(new ParseResult(template, Mustache.Parse(template)))
        .UseDirectory("Snapshots\\Mustache");

    private record ParseResult(string Template, DataOrDiagnostics<Mustache.TemplateDefinition> Result);
}
