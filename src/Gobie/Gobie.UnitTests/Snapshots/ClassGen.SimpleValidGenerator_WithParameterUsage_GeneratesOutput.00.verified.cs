//HintName: _Gobie.NamePropertyAttribute.g.cs
namespace Gobie
{
    /// <summary>
    /// This attribute will cause the generator <see 
    ///cref = "Gobie.NamePropertyGenerator"/> to run.
    /// </summary>
    public sealed class NamePropertyAttribute : global::Gobie.GobieClassGeneratorAttribute
    {
        public NamePropertyAttribute(string initialName, int id)
        {
            this.InitialName = initialName;
            this.Id = id;
        }

        public string InitialName { get; }

        public int Id { get; }
    }
}