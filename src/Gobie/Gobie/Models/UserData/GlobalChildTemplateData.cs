namespace Gobie.Models.UserData
{
    public class GlobalChildTemplateData
    {
        public GlobalChildTemplateData(string templateName, Mustache.TemplateDefinition template)
        {
            GlobalTemplateName = templateName;
            Template = template;
        }

        public string GlobalTemplateName { get; }

        public Mustache.TemplateDefinition Template { get; }
    }
}
