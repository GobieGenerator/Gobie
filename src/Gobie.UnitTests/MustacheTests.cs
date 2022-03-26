namespace Gobie.UnitTests;

using System.Collections.Immutable;

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

    [TestCase("{{#name}}Hello {{name}}{{/name}} {{^name}}No one is here{{/name}}")]
    [TestCase("Hello {{name}} {{#name}}Who is a person {{#job}}with a job: {{job}}{{/job}}{{/name}}")]
    [TestCase("Hello {{name}} {{#name}}\n\nWho is a person {{#foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("Hello {{foo}} {{#name}}Who is a person {{#foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("Hello {{foo}} {{#name}}Who is a person {{^foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("private string {{name}}(string greeting) \n{\n return $\"{greeting}: {{name}}\";\n}\n")]
    public Task Render_Succeeds(string template)
    {
        var data = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();

        data.Add("name", new Mustache.RenderData("name", "Mike", true));
        data.Add("job", new Mustache.RenderData("job", "programmer", true));
        data.Add("foo", new Mustache.RenderData("foo", "", false));

        var parse = Mustache.Parse(template);
        var render = Mustache.RenderTemplate(parse.Data!, data.ToImmutable());

        return Verify(render).UseDirectory("Snapshots\\Mustache\\Render");
    }

    private record ParseResult(string Template, DataOrDiagnostics<Mustache.TemplateDefinition> Result);
}
