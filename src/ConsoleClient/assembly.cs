using ConsoleClient;
using System;

//[assembly: MustacheAttribute("TemplateTest", Template.TestTemplate, "foooo")]
[assembly: CustomAssemblyGenerator("TemplateTest", Template.TestTemplate)]
[assembly: CustomAssemblyGeneratorAttribute("TemplateTest", Template.TestTemplate)]


public class Template
{
    public const string TestTemplate = @"
    public class SayHi {
        public static void ToSomeone() => System.Console.WriteLine(""Hi {{Name}} {{Name}}"");
    }
";
}
