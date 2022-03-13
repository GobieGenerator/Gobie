namespace Gobie;

using Microsoft.CodeAnalysis;

public class DataOrDiagnostics<T>
{
    public DataOrDiagnostics(T data, IEnumerable<Diagnostic>? diagnostics = null)
    {
        Data = data;

        if (diagnostics is not null && diagnostics.Any())
        {
            Diagnostics = new List<Diagnostic>(diagnostics);
        }
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
