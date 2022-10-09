namespace Gobie.Models
{
    public static class Diagnostics
    {
        private const string BaseHelpLink = "https://github.com/GobieGenerator/Gobie/tree/main/Docs/Diagnostics#";

        public static DiagnosticDescriptor ClassIsNotParital =>
            Error("GB1001", "Class must be defined as partial.");

        public static DiagnosticDescriptor TemplateIsNotConstString =>
            Error("GB1002", "Template must annotate constant strings.");

        public static DiagnosticDescriptor GobieAttributeIsPartial =>
            Error("GB1003", "Gobie Attributes cannot be implemented with partial classes. As a reminder, multiple source generators can't interact.");

        public static DiagnosticDescriptor UserTemplateIsPartial =>
            Error("GB1004", "Classes that declare user templates cannot be partial. We don't current support user templates having source in multiple files.");

        public static DiagnosticDescriptor UserTemplateIsNotSealed =>
            Error("GB1005", "Classes that declare user templates must be sealed. We don't currently support inheritance of user templates.");

        public static DiagnosticDescriptor GeneratorNameInvalid =>
            Error("GB1006", "Generator names are expected to end with 'Generator'. You may use th....");

        public static DiagnosticDescriptor GobieGlobalTemplateIdentifierIssue =>
            Error("GB1007", $"Gobie should have zero or one instance of the '{{{{ChildContent}}}}'. No other identifiers are allowed in global templates.");

        public static DiagnosticDescriptor GobieAttributeHasNoTemplates =>
            Warning("GB1008", "Attribute has no tempaltes and will not generate any output.");

        public static DiagnosticDescriptor DisallowedTemplateParameterType(string typeName) =>
            Error("GB1009", $"The specified type name '{typeName}' is not one of the types supported by Gobie.");

        public static DiagnosticDescriptor UnexpectedToken(string token, string expected) =>
            Error("GB1010", $"The token '{token}' was not expected. {expected}");

        public static DiagnosticDescriptor MissingToken(string expected) =>
            Error("GB1011", $"The expected token '{expected}' was not found.");

        public static DiagnosticDescriptor UnexpectedIdentifier(string token, string expected) =>
            Error("GB1012", $"The identifier '{token}' was not expected. {expected}");

        public static DiagnosticDescriptor LogicalEndMissing(string details) =>
            Error("GB1013", $"The template is missing a closing tag. We expect '{details}' at or before this point in the template.");

        public static DiagnosticDescriptor UnfinishedTemplate(string details) =>
            Error("GB1014", $"The template is incomplete. {details}");

        public static DiagnosticDescriptor UnreachableTemplateSection(string details) =>
            Error("GB1015", $"This section of the template will never be reached. {details}");

        public static DiagnosticDescriptor GobieCrashed(string exMessage) =>
            Error("GB1016", $"Gobie Crashed. {exMessage}");

        public static DiagnosticDescriptor GobieUnknownError(string exMessage) =>
            Error("GB1017", $"Gobie had some error. {exMessage}");

        public static DiagnosticDescriptor UserTemplateIsEmpty(string className) =>
            Warning("GB1018", $"The generator {className} does not have any templates defined. Therefore it will not generate any code.");

        public static DiagnosticDescriptor PriorityAlreadyDeclared(int i) =>
            Warning("GB1019", $"Another required parameter is using the priority {i}");

        private static DiagnosticDescriptor Error(string code, string message) => FullDiscriptor(code, message, DiagnosticSeverity.Error);

        private static DiagnosticDescriptor Warning(string code, string message) => FullDiscriptor(code, message, DiagnosticSeverity.Warning);

        private static DiagnosticDescriptor FullDiscriptor(string code, string message, DiagnosticSeverity severity) =>
            new(code, "Gobie", message, "Gobie", severity, true, helpLinkUri: BaseHelpLink + code);
    }
}
