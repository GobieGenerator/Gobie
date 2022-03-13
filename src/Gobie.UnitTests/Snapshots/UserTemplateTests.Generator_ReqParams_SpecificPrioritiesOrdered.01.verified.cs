//HintName: _PrimaryKeyGeneratorAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "TODONAMESPACE.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyGeneratorAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyGeneratorAttribute(int reqRequestedFirst, string reqRequestedFifth)
        {
            this.ReqRequestedFirst = reqRequestedFirst;
            this.ReqRequestedFifth = reqRequestedFifth;
        }

        public int ReqRequestedFirst { get; set; }

        public string ReqRequestedFifth { get; set; }
    }
}