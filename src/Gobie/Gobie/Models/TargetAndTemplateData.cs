namespace Gobie.Models;

using Gobie.Enums;
using System;

/// <summary>
/// Has a 1:1 mapping of templates and the targets they are mapped to.
/// </summary>
public class TargetAndTemplateData
{
    public TargetAndTemplateData(TemplateType templateType, string generatorName, ClassIdentifier targetClass, string code)
    {
        TemplateType = templateType;
        GeneratorName = generatorName ?? throw new ArgumentNullException(nameof(generatorName));
        TargetClass = targetClass;
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    public TemplateType TemplateType { get; }

    public string GeneratorName { get; }

    public ClassIdentifier TargetClass { get; }

    public string Code { get; }
}
