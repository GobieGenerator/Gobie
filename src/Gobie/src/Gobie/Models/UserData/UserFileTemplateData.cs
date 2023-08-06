namespace Gobie.Models.UserData;

public class UserFileTemplateData
{
    public UserFileTemplateData(string fileName, Mustache.TemplateDefinition template)
    {
        FileName = fileName ?? string.Empty;
        Template = template;
    }

    public string FileName { get; }

    public Mustache.TemplateDefinition Template { get; }
}
