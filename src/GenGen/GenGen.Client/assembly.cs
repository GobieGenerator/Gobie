[assembly: MustacheAttribute("TemplateTest", Template.TestTemplate, "{\"Name\": \"Mike\"}")]

public class Template
{
    public const string TestTemplate = @"
public class TemplateOutput {
    public static string Name => {{Name}}{{Name}}{{Name}};
}
";
}
