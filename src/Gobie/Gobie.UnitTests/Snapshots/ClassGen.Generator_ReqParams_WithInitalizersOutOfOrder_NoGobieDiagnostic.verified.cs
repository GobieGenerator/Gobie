﻿//HintName: _Gobie.PrimaryKeyAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "Gobie.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyAttribute : global::Gobie.GobieClassGeneratorAttribute
    {
        public PrimaryKeyAttribute(int myInt = 4, string myString)
        {
            this.MyInt = myInt;
            this.MyString = myString;
        }

        public int MyInt { get; }

        public string MyString { get; }
    }
}