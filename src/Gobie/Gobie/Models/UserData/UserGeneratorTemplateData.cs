namespace Gobie.Models.UserData;

/// <summary>
/// Class that contains all the data about the generator required to run the generator.
/// </summary>
public class UserGeneratorTemplateData
{
    public UserGeneratorTemplateData(
        UserGeneratorAttributeData data,
        List<Mustache.TemplateDefinition> templates,
        List<UserFileTemplateData> fileTemplates,
        List<GlobalTemplateData> globalTemplateDefs)
    {
        AttributeData = data;
        Templates = templates;
        FileTemplates = fileTemplates;
        GlobalTemplate = globalTemplateDefs;
    }

    public List<Mustache.TemplateDefinition> Templates { get; }

    public UserGeneratorAttributeData AttributeData { get; }

    public List<UserFileTemplateData> FileTemplates { get; }

    public List<GlobalTemplateData> GlobalTemplate { get; }
}
