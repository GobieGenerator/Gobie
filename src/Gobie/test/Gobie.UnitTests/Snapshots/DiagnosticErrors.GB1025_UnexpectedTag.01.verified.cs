//HintName: _Gobie.UserDefinedAttribute.g.cs
namespace Gobie
{
    /// <summary>
    /// This attribute will cause the generator <see 
    ///cref = "Gobie.UserDefinedGenerator"/> to run.
    /// </summary>
    public sealed class UserDefinedAttribute : global::Gobie.GobieClassGeneratorAttribute
    {
        public UserDefinedAttribute()
        {
        }

        public string ImExpected { get; set; }
    }
}