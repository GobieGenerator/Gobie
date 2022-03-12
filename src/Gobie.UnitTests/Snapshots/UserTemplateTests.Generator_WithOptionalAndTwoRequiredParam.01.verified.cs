//HintName: PrimaryKeyGeneratorAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "TODONAMESPACE.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyGeneratorAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyGeneratorAttribute(string reqParam, int reqParamTheSecond)
        {
            this.ReqParam = reqParam;
            this.ReqParamTheSecond = reqParamTheSecond;
        }

        public string ReqParam { get; set; }

        public int ReqParamTheSecond { get; set; }

        public string MyParam { get; set; }

        public string OtherParam { get; set; } = "My Initial Value";
    }
}