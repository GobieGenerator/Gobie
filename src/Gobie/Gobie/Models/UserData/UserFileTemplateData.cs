namespace Gobie.Models.UserData;

public class UserFileTemplateData
{
    public UserFileTemplateData(string fileName, Mustache.TemplateDefinition template)
    {
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Template = template ?? throw new ArgumentNullException(nameof(template));
    }

    public string FileName { get; }

    public Mustache.TemplateDefinition Template { get; }
}
