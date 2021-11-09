[assembly: MustacheAttribute("TemplateTest", Template.TestTemplate, "foooo")]

public class Template
{
    public const string TestTemplate = @"
    public class SayHi {
        public static void ToSomeone() => System.Console.WriteLine(""Hi {{Name}} {{Name}} {{Name}}"");
    }
";
}
