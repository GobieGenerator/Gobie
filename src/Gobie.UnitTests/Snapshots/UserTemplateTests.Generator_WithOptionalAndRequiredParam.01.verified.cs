﻿//HintName: PrimaryKeyGeneratorAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "TODONAMESPACE.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyGeneratorAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyGeneratorAttribute(string reqParam)
        {
            this.ReqParam = reqParam;
        }

        public string ReqParam { get; set; }

        public string MyParam { get; set; }

        public string OtherParam { get; set; } = "My Initial Value";
    }
}