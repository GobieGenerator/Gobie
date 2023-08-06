namespace Gobie.Models.UserData;

public class GlobalTemplateData
{
    public GlobalTemplateData(string templateName, Mustache.TemplateDefinition template)
    {
        TemplateName = templateName;
        Template = template;
    }

    public string TemplateName { get; }

    public Mustache.TemplateDefinition Template { get; }
}
