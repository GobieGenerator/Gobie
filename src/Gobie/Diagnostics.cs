using Microsoft.CodeAnalysis;

namespace Gobie
{
    public static class Diagnostics
    {
        public static DiagnosticDescriptor ClassIsNotParital =>
             new("GB0001", "Gobie", "Class must be defined as partial.", "Gobie", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor TemplateIsNotConstString =>
             new("GB0002", "Gobie", "Template must annotate constant strings.", "Gobie", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor GobieAttributeIsPartial =>
             new("GB0003", "Gobie", "Gobie Attributes cannot be implemented with partial classes. As a reminder, multiple source generators can't interact.", "Gobie", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor GobieAttributeHasNoTemplates =>
             new("GB1003", "Gobie", "Attribute has no tempaltes and will not generate any output.", "Gobie", DiagnosticSeverity.Warning, true);

        public static DiagnosticDescriptor UserTemplateIsPartial =>
             new("GB0010", "Gobie", "Classes that declare user templates cannot be partial. We don't current support user templates having source in multiple files.", "Gobie Usage", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor UserTemplateIsNotSealed =>
             new("GB0010", "Gobie", "Classes that declare user templates must be sealed. We don't currently support inheritance of user templates.", "Gobie Usage", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor GobieCrashed(string exMessage) =>
                             new("GB0000", "Gobie", $"Gobie Crashed. {exMessage}", "Gobie", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor GobieUnknownError(string exMessage) =>
             new("GB0000", "Gobie", $"Gobie had some error. {exMessage}", "Gobie", DiagnosticSeverity.Error, true);
    }
}
