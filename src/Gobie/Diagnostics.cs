using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobie
{
    public static class Diagnostics
    {
        public static DiagnosticDescriptor ClassIsNotParital =>
             new DiagnosticDescriptor("GB0001", "Gobie", "Class must be defined as partial.", "Gobie", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor TemplateIsNotConstString =>
             new DiagnosticDescriptor("GB0002", "Gobie", "Template must annotate constant strings.", "Gobie", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor GobieAttributeIsPartial =>
             new DiagnosticDescriptor("GB0003", "Gobie", "Gobie Attributes cannot be implemented with partial classes. As a reminder, multiple source generators can't interact.", "Gobie", DiagnosticSeverity.Error, true);

        public static DiagnosticDescriptor GobieAttributeHasNoTemplates =>
             new DiagnosticDescriptor("GB1003", "Gobie", "Attribute has no tempaltes and will not generate any output.", "Gobie", DiagnosticSeverity.Warning, true);
    }
}
