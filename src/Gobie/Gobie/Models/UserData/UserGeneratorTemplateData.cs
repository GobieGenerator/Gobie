namespace Gobie.Models.UserData;

/// <summary>
/// Class that contains all the data about the generator required to run the generator.
/// </summary>
public class UserGeneratorTemplateData
{
    public UserGeneratorTemplateData(
        UserGeneratorAttributeData data,
        List<Mustache.TemplateDefinition> templates,
        List<UserFileTemplateData> fileTemplates)
    {
        FileTemplates = fileTemplates;
        AttributeData = data;
        Templates = templates;
    }

    public List<Mustache.TemplateDefinition> Templates { get; }

    public UserGeneratorAttributeData AttributeData { get; }

    public List<UserFileTemplateData> FileTemplates { get; }
}
