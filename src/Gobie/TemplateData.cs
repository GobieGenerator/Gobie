namespace Gobie
{
    using Microsoft.CodeAnalysis;

    public class TemplateData
    {
    }

    public class DataOrDiagnostics<T>
    {
        public DataOrDiagnostics(T data)
        {
            Data = data;
        }

        public DataOrDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
            Diagnostics = new List<Diagnostic>(diagnostics);
        }

        public DataOrDiagnostics(Diagnostic diagnostic)
        {
            Diagnostics = new List<Diagnostic>
            {
                diagnostic
            };
        }

        public T? Data { get; }

        public IReadOnlyList<Diagnostic>? Diagnostics { get; }
    }
}
