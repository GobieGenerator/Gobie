namespace Gobie.Models.Diagnostics
{
    public static class Errors
    {
        private static readonly DiagnosticSeverity Severity = DiagnosticSeverity.Error;

        public static DiagnosticDescriptor ClassIsNotParital =>
            new("GB0001", "Gobie", "Class must be defined as partial.", "Gobie", Severity, true);

        public static DiagnosticDescriptor TemplateIsNotConstString =>
            new("GB0002", "Gobie", "Template must annotate constant strings.", "Gobie", Severity, true);

        public static DiagnosticDescriptor GobieAttributeIsPartial =>
            new("GB0003", "Gobie", "Gobie Attributes cannot be implemented with partial classes. As a reminder, multiple source generators can't interact.", "Gobie", Severity, true);

        public static DiagnosticDescriptor UserTemplateIsPartial =>
            new("GB0010", "Gobie", "Classes that declare user templates cannot be partial. We don't current support user templates having source in multiple files.", "Gobie Usage", Severity, true);

        public static DiagnosticDescriptor UserTemplateIsNotSealed =>
            new("GB0011", "Gobie", "Classes that declare user templates must be sealed. We don't currently support inheritance of user templates.", "Gobie Usage", Severity, true);

        public static DiagnosticDescriptor GeneratorNameInvalid =>
            new("GB0012", "Gobie", "Generator names are expected to end with 'Generator'. You may use th....", "Gobie Usage", Severity, true);

        public static DiagnosticDescriptor DisallowedTemplateParameterType(string typeName) =>
            new("GB1001", "Gobie", $"The specified type name '{typeName}' is not one of the types supported by Gobie.", "Gobie", Severity, true);

        public static DiagnosticDescriptor UnexpectedToken(string token, string expected) =>
            new("GB1001", "Gobie", $"The token '{token}' was not expected. {expected}", "Gobie", Severity, true);

        public static DiagnosticDescriptor MissingToken(string expected) =>
            new("GB1001", "Gobie", $"The expected token '{expected}' was not found.", "Gobie", Severity, true);

        public static DiagnosticDescriptor UnexpectedIdentifier(string token, string expected) =>
            new("GB1001", "Gobie", $"The identifier '{token}' was not expected. {expected}", "Gobie", Severity, true);

        public static DiagnosticDescriptor LogicalEndMissing(string details) =>
            new("GB1001", "Gobie", $"The template is missing a closing tag. We expect '{details}' at or before this point in the template.", "Gobie", Severity, true);

        public static DiagnosticDescriptor UnfinishedTemplate(string details) =>
            new("GB1001", "Gobie", $"The template is incomplete. {details}", "Gobie", Severity, true);

        public static DiagnosticDescriptor UnreachableTemplateSection(string details) =>
            new("GB1001", "Gobie", $"This section of the template will never be reached. {details}", "Gobie", Severity, true);

        public static DiagnosticDescriptor GobieCrashed(string exMessage) =>
            new("GB1001", "Gobie", $"Gobie Crashed. {exMessage}", "Gobie", Severity, true);

        public static DiagnosticDescriptor GobieUnknownError(string exMessage) =>
            new("GB1002", "Gobie", $"Gobie had some error. {exMessage}", "Gobie", Severity, true);
    }

    public static class Warnings
    {
        private static readonly DiagnosticSeverity Severity = DiagnosticSeverity.Warning;

        public static DiagnosticDescriptor UserTemplateIsEmpty =>
            new("GB0012", "Gobie", "You have not defined any of the required info.... The generator won't be able to do anything.", "Gobie Usage", Severity, true);

        public static DiagnosticDescriptor GobieAttributeHasNoTemplates =>
            new("GB1003", "Gobie", "Attribute has no tempaltes and will not generate any output.", "Gobie", Severity, true);

        public static DiagnosticDescriptor PriorityAlreadyDeclared(int i) =>
                new("GB0012", "Gobie", $"Another required parameter is using the priority {i}", "Gobie Usage", Severity, true);
    }
}
