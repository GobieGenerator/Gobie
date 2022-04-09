//HintName: _Gobie.NamePropertyAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "Gobie.NamePropertyGenerator"/> to run. </summary>
    public sealed class NamePropertyAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public NamePropertyAttribute(string initialName)
        {
            this.InitialName = initialName;
        }

        public string InitialName { get; }

        public int Id { get; set; }
    }
}