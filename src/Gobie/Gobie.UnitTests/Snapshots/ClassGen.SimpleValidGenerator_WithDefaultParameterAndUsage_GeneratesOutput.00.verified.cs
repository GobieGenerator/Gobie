//HintName: _Gobie.NamePropertyAttribute.g.cs
namespace Gobie
{
    /// <summary>
    /// This attribute will cause the generator <see 
    ///cref = "Gobie.NamePropertyGenerator"/> to run.
    /// </summary>
    public sealed class NamePropertyAttribute : global::Gobie.GobieClassGeneratorAttribute
    {
        public NamePropertyAttribute(string initialName, int person_Id = 2048234)
        {
            this.InitialName = initialName;
            this.Person_Id = person_Id;
        }

        public string InitialName { get; }
        public int Person_Id { get; }
    }
}