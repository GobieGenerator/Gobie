//HintName: _Gobie.PrimaryKeyAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "Gobie.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyAttribute : global::Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyAttribute(string myString, int myInt = 4)
        {
            this.MyString = myString;
            this.MyInt = myInt;
        }

        public string MyString { get; }

        public int MyInt { get; }
    }
}