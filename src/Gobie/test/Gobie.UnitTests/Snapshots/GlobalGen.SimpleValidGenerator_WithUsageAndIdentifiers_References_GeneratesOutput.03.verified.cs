//HintName: OtherTarget_EncapsulatedCollectionAttribute.g.cs
namespace SomeNamespace
{
    public partial class OtherTarget
    {
        public IEnumerable<string> Names => names.AsReadOnly(); // Encapsulating List<string> 
    }
}