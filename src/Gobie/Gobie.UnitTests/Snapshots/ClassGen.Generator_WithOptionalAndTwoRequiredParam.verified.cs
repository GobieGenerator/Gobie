//HintName: _Gobie.PrimaryKeyAttribute.g.cs
namespace Gobie
{
    /// <summary>
    /// This attribute will cause the generator <see 
    ///cref = "Gobie.PrimaryKeyGenerator"/> to run.
    /// </summary>
    public sealed class PrimaryKeyAttribute : global::Gobie.GobieClassGeneratorAttribute
    {
        public PrimaryKeyAttribute(string reqParam, int reqParamTheSecond)
        {
            this.ReqParam = reqParam;
            this.ReqParamTheSecond = reqParamTheSecond;
        }

        public string ReqParam { get; }
        public int ReqParamTheSecond { get; }
        public string MyParam { get; set; }
        public string OtherParam { get; set; } = "My Initial Value";
    }
}