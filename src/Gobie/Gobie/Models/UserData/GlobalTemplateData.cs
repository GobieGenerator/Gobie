namespace Gobie.Models.UserData
{
    public class GlobalTemplateData
    {
        public GlobalTemplateData(string templateName, string fileName, Mustache.TemplateDefinition template)
        {
            TemplateName = templateName;
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Template = template ?? throw new ArgumentNullException(nameof(template));
        }

        public string TemplateName { get; }

        public string FileName { get; }

        public Mustache.TemplateDefinition Template { get; }
    }
}
