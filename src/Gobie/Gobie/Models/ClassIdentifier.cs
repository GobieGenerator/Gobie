namespace Gobie.Models;

using System.Collections.Generic;

public readonly struct ClassIdentifier : IEquatable<ClassIdentifier>
{
    public ClassIdentifier(string classNamespace, string className)
    {
        NamespaceName = classNamespace ?? string.Empty;
        ClassName = className ?? string.Empty;
    }

    public string NamespaceName { get; }

    public string ClassName { get; }

    public string FullName =>
    $"{NamespaceName}{(string.IsNullOrWhiteSpace(NamespaceName) ? "" : ".")}{ClassName}";

    public string GlobalName => $"global::{FullName}";

    public override bool Equals(object? obj)
    {
        return obj is ClassIdentifier identifier && Equals(identifier);
    }

    public bool Equals(ClassIdentifier other)
    {
        return NamespaceName == other.NamespaceName &&
               ClassName == other.ClassName;
    }

    public override int GetHashCode()
    {
        int hashCode = 1117777763;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NamespaceName);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ClassName);
        return hashCode;
    }
}
