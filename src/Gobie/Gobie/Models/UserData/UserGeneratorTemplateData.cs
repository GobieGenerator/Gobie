namespace Gobie.Models.UserData;

/// <summary>
/// Class that contains all the data about the generator required to run the generatorion.
/// </summary>
public class UserGeneratorTemplateData
{
    public UserGeneratorTemplateData(UserGeneratorAttributeData data, List<Mustache.Mustache.TemplateDefinition> templates)
    {
        AttributeData = data;
        Templates = templates;
    }

    public List<Mustache.Mustache.TemplateDefinition> Templates { get; }

    public UserGeneratorAttributeData AttributeData { get; }
}
