namespace Gobie.UnitTests;

public class MustacheTests
{
    [Test]
    public Task SimpleAst()
    {
        const string template = "\n {{ name }}  ";

        var ast = Mustache.Parse(template, Mustache.Tokenize(template));

        return Verify(new ParseResult(template, ast));
    }

    private record ParseResult(string Template, DataOrDiagnostics<Mustache.TemplateSyntax> Result);
}
