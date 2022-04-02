//HintName: _Gobie.PrimaryKeyAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "Gobie.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyAttribute(string reqRequestedFifth, int reqRequestedFirst, int alsoReqRequestedFifth, int anotherReqRequestedFifth)
        {
            this.ReqRequestedFifth = reqRequestedFifth;
            this.ReqRequestedFirst = reqRequestedFirst;
            this.AlsoReqRequestedFifth = alsoReqRequestedFifth;
            this.AnotherReqRequestedFifth = anotherReqRequestedFifth;
        }

        public string ReqRequestedFifth { get; }

        public int ReqRequestedFirst { get; }

        public int AlsoReqRequestedFifth { get; }

        public int AnotherReqRequestedFifth { get; }
    }
}