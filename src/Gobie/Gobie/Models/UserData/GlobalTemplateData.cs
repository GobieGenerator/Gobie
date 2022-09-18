namespace Gobie.Models.UserData
{
    public class GlobalTemplateData
    {
        public GlobalTemplateData(string templateName, string fileName, Mustache.TemplateDefinition template)
        {
            TemplateName = templateName;
            FileName = fileName ?? string.Empty;
            Template = template;
        }

        public string TemplateName { get; }

        public string FileName { get; }

        public Mustache.TemplateDefinition Template { get; }
    }
}
