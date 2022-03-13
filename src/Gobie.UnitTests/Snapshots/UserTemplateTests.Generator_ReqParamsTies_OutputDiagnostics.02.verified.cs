//HintName: _PrimaryKeyGeneratorAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "TODONAMESPACE.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyGeneratorAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyGeneratorAttribute(int reqRequestedFirst, string reqRequestedFifth, int alsoReqRequestedFifth, int anotherReqRequestedFifth)
        {
            this.ReqRequestedFirst = reqRequestedFirst;
            this.ReqRequestedFifth = reqRequestedFifth;
            this.AlsoReqRequestedFifth = alsoReqRequestedFifth;
            this.AnotherReqRequestedFifth = anotherReqRequestedFifth;
        }

        public int ReqRequestedFirst { get; set; }

        public string ReqRequestedFifth { get; set; }

        public int AlsoReqRequestedFifth { get; set; }

        public int AnotherReqRequestedFifth { get; set; }
    }
}