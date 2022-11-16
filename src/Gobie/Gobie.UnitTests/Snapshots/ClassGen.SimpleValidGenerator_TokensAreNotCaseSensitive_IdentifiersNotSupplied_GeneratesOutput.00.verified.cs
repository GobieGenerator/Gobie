//HintName: _Gobie.NamePropertyAttribute.g.cs
namespace Gobie
{
    /// <summary>
    /// This attribute will cause the generator <see 
    ///cref = "Gobie.NamePropertyGenerator"/> to run.
    /// </summary>
    public sealed class NamePropertyAttribute : global::Gobie.GobieClassGeneratorAttribute
    {
        public NamePropertyAttribute()
        {
        }

        public string InitialName { get; set; }

        public int Id { get; set; }
    }
}