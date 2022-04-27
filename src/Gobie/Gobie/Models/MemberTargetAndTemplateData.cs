namespace Gobie.Models;

using Gobie.Enums;
using System;

/// <summary>
/// Has a 1:1 mapping of templates and the targets they are mapped to.
/// </summary>
public class MemberTargetAndTemplateData
{
    public MemberTargetAndTemplateData(TemplateType templateType, string generatorName, ClassIdentifier targetClass, string code)
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

/// <summary>
/// Has a 1:1 mapping of templates and the targets they are mapped to.
/// </summary>
public class AssemblyTargetAndTemplateData
{
    public AssemblyTargetAndTemplateData(string globalGeneratorName, Mustache.TemplateDefinition globalTemplate)
    {
        GlobalGeneratorName = globalGeneratorName ?? throw new ArgumentNullException(nameof(globalGeneratorName));
        GlobalTemplate = globalTemplate ?? throw new ArgumentNullException(nameof(globalTemplate));
    }

    public string GlobalGeneratorName { get; }

    public Mustache.TemplateDefinition GlobalTemplate { get; }
}
