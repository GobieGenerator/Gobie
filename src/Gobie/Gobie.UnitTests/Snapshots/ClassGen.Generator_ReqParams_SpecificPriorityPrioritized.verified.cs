//HintName: _Gobie.PrimaryKeyAttribute.g.cs
namespace Gobie
{
    /// <summary> This attribute will cause the generator defined by this thing here to
    /// run <see cref = "Gobie.PrimaryKeyGenerator"/> to run. </summary>
    public sealed class PrimaryKeyAttribute : global::Gobie.GobieFieldGeneratorAttribute
    {
        public PrimaryKeyAttribute(int reqRequestedFirst, string reqNoPositionRequest)
        {
            this.ReqRequestedFirst = reqRequestedFirst;
            this.ReqNoPositionRequest = reqNoPositionRequest;
        }

        public int ReqRequestedFirst { get; }

        public string ReqNoPositionRequest { get; }
    }
}