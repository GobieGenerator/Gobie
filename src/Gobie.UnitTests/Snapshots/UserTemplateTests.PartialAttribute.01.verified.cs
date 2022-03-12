//HintName: PartialNameAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "TODONAMESPACE.PartialName"/> to run. </summary>
    public sealed class PartialNameAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public PartialNameAttribute()
        {
        }
    }
}