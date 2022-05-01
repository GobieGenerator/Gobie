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
    [TestCase("{{ a b }}")]
    [TestCase("{{ a pascal }}")]
    [TestCase("{{ a camel }}")]
    public Task Parse_InvalidFlatAst(string template) => TestParsing(template);

    [TestCase("{{^name}} Text1 Text2")]
    public Task Parse_Partial_IssuesDiagnostic(string template) => TestParsing(template);

    [TestCase("")]
    [TestCase("just some text")]
    [TestCase("\n {{ name }}  ")]
    [TestCase("{{#name}}Something{{/name}} {{^name}}Something Else{{/name}}")]
    [TestCase("{{#name}}Someone named {{name}} {{#age}} with age of {{age}}{{/age}} {{/name}} is great!!")]
    [TestCase("{{#name}} Text1 {{#foo}}{{#name}}{{/name}} Text2  {{/foo}}{{/name}}")] // Inner if name is redundant.
    [TestCase("private void {{name}}(string greeting) => {{name}}.Add(greeting);")]
    [TestCase("{{name:pascal}}")]
    [TestCase("{{name:pascal}}{{name:camel}}{{ name :  PASCAL }}{{ name : CAMEL  }}")]
    public Task Parse_Succeeds(string template) => TestParsing(template);

    [TestCase("{{^name}} Text1 {{name}} Text2 {{/name}}")]
    [TestCase("{{^name}} Text1 {{#name}}{{/name}} Text2 {{/name}}")]
    [TestCase("{{^name}} Text1 {{#foo}}{{name}} Text2  {{/foo}}{{/name}}")]
    [TestCase("{{^name}} Text1 {{#foo}}{{#name}}{{/name}} Text2  {{/foo}}{{/name}}")]
    [TestCase("{{#name}} Text1 {{#foo}}{{^name}}{{/name}} Text2  {{/foo}}{{/name}}")]
    public Task Parse_WithInvalidLogic_IssuesDiagnostics(string template) => TestParsing(template);

    [TestCase("{{#name}}Hello {{name}}{{/name}} {{^name}}No one is here{{/name}}")]
    [TestCase("Hello {{name}} {{#name}}Who is a person {{#job}}with a job: {{job}}{{/job}}{{/name}}")]
    [TestCase("Hello {{name}} {{#name}}\n\nWho is a person {{#foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("Hello {{foo}} {{#name}}Who is a person {{#foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("Hello {{foo}} {{#name}}Who is a person {{^foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("private string {{name}}(string greeting) \n{\n return $\"{greeting}: {{name}}\";\n}\n")]
    [TestCase("private void {{name}}(string greeting) => {{name}}.Add(greeting);")]
    [TestCase("private void {{name : Pascal}}(string greeting) => {{name:camel}}.Add({{job:pascal}} = {{job:camel}});")]
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

    [TestCase("{{#name}}Hello {{name}}{{/name}} {{^name}}No one is here{{/name}}")]
    [TestCase("Hello {{name}} {{#name}}Who is a person {{#job}}with a job: {{job}}{{/job}}{{/name}}")]
    [TestCase("Hello {{name}} {{#name}}\n\nWho is a person {{#foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("Hello {{foo}} {{#name}}Who is a person {{#foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("Hello {{foo}} {{#name}}Who is a person {{^foo}}with a job: {{job}}{{/foo}}{{/name}}")]
    [TestCase("private string {{name}}(string greeting) \n{\n return $\"{greeting}: {{name}}\";\n}\n")]
    public void Render_MissingData_DoesNotThrow(string template)
    {
        // Its important that if we for some reason render a template missing data that we don't
        // crash the generator.
        var data = ImmutableDictionary.CreateBuilder<string, Mustache.RenderData>();
        var parse = Mustache.Parse(template);
        Assert.DoesNotThrow(() => Mustache.RenderTemplate(parse.Data!, data.ToImmutable()));
    }

    private static Task TestParsing(string template)
    {
        var parsed = Mustache.Parse(template);

        return Verify(new ParseResult(template, new(parsed.Data, parsed.Diagnostics)))
              .UseDirectory("Snapshots\\Mustache\\Parsing");
    }

    private record ParseResult(string Template, DataOrDiagnostics<SeralizableTemplateDef> Result);

    /// <summary>
    /// The <see cref="Mustache.TemplateDefinition.Identifiers"/> does not seralize in a
    /// determinstic order. So this is here as a helper for verification.
    /// </summary>
    private class SeralizableTemplateDef
    {
        private readonly Mustache.TemplateDefinition? definition;

        public SeralizableTemplateDef(Mustache.TemplateDefinition? definition)
        {
            this.definition = definition;
        }

        public Mustache.TemplateSyntax? Syntax => definition?.Syntax;

        public IEnumerable<string>? Identifiers => definition?.Identifiers.OrderBy(x => x);

        public static implicit operator SeralizableTemplateDef(Mustache.TemplateDefinition? definition)
        {
            return new SeralizableTemplateDef(definition);
        }
    }
}
