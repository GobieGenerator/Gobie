//HintName: _Gobie.UserDefinedAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "Gobie.UserDefinedGenerator"/> to run. </summary>
    public sealed class UserDefinedAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public UserDefinedAttribute()
        {
        }
    }
}