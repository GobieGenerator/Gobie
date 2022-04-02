//HintName: _Gobie.PrimaryKeyAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "Gobie.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyAttribute : Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyAttribute(string reqNoPositionRequest, int reqRequestedFirst)
        {
            this.ReqNoPositionRequest = reqNoPositionRequest;
            this.ReqRequestedFirst = reqRequestedFirst;
        }

        public string ReqNoPositionRequest { get; }

        public int ReqRequestedFirst { get; }
    }
}