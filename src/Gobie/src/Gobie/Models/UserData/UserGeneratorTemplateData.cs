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
        List<GlobalTemplateData> globalTemplateDefs,
        List<GlobalChildTemplateData> globalChildTemplateDefs)
    {
        AttributeData = data;
        Templates = templates;
        FileTemplates = fileTemplates;
        GlobalTemplate = globalTemplateDefs;
        GlobalChildTemplates = globalChildTemplateDefs;
    }

    public List<Mustache.TemplateDefinition> Templates { get; }

    public UserGeneratorAttributeData AttributeData { get; }

    public List<UserFileTemplateData> FileTemplates { get; }

    public List<GlobalTemplateData> GlobalTemplate { get; }

    public List<GlobalChildTemplateData> GlobalChildTemplates { get; }

    /// <summary>
    /// Helper to return whether there is any defined template, of any kind, for this generator.
    /// </summary>
    public bool HasAnyTemplate =>
        Templates.Any() || FileTemplates.Any() || GlobalTemplate.Any() || GlobalChildTemplates.Any();
}
