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
    }
}
