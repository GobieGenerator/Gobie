﻿namespace Gobie.UnitTests;

public class MustacheTests
{
    [TestCase("")]
    [TestCase("just some text")]
    [TestCase("\n {{ name }}  ")]
    public Task Parse_ValidFlatAst(string template) =>
        Verify(new ParseResult(template, Mustache.Parse(template)))
            .UseDirectory("Snapshots\\Mustache");

    private record ParseResult(string Template, DataOrDiagnostics<Mustache.TemplateSyntax> Result);
}
