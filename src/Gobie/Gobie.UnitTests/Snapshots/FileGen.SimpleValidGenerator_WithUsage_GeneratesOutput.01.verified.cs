//HintName: GenTarget_LoggedClassAttribute_Log.g.cs
namespace SomeNamespace;
public sealed class GenTargetLog
{
    public int Id { get; set; }

    public GenTarget Parent { get; set; }

    public DateTime Timestamp { get; set; }

    public string LogMessage { get; set; }
}