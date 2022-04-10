namespace Gobie.Models;

public readonly struct CodeOutput
{
    public CodeOutput(string hintName, string code)
    {
        HintName = hintName ?? throw new ArgumentNullException(nameof(hintName));
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    public string HintName { get; }

    public string Code { get; }
}
