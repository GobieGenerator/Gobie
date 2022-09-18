namespace Gobie.Models;

public readonly struct CodeOutput
{
    public CodeOutput(string hintName, string code)
    {
        HintName = hintName ?? string.Empty; //TODO is this ok?
        Code = code ?? string.Empty;
    }

    public string HintName { get; }

    public string Code { get; }
}
