namespace Gobie
{
    public static class SourceGenerationHelper
    {
        public const string Attribute = @"
namespace NetEscapades.EnumGenerators
{
// test
    [System.AttributeUsage(System.AttributeTargets.Enum)]
    public class EnumExtensionsAttribute : System.Attribute
    {
    }
}";
    }
}
