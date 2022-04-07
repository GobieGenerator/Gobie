namespace Gobie.Models;

public class ClassIdentifier
{
    public ClassIdentifier(string classNamespace, string className)
    {
        NamespaceName = classNamespace ?? throw new ArgumentNullException(nameof(classNamespace));
        ClassName = className ?? throw new ArgumentNullException(nameof(className));
    }

    public string NamespaceName { get; }

    public string ClassName { get; }

    public string FullName =>
    $"{NamespaceName}{(string.IsNullOrWhiteSpace(NamespaceName) ? "" : ".")}{ClassName}";

    public string GlobalName =>
        $"global::{FullName}";
}
