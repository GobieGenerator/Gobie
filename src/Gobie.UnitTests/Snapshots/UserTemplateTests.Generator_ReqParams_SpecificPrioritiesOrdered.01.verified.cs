//HintName: _Gobie.PrimaryKeyAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "Gobie.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyAttribute(int reqRequestedFirst, string reqRequestedFifth)
        {
            this.ReqRequestedFirst = reqRequestedFirst;
            this.ReqRequestedFifth = reqRequestedFifth;
        }

        public int ReqRequestedFirst { get; set; }

        public string ReqRequestedFifth { get; set; }
    }
}