[assembly: MustacheAttribute("TemplateTest", Template.TestTemplate, "")]

public class Template
{
    public const string TestTemplate = @"
public class TemplateOutput {}
";
}
