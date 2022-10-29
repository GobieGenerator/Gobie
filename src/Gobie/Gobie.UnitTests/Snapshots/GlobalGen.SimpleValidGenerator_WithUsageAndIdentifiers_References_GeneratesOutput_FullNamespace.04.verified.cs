//HintName: TemplateTarget_EncapsulatedCollectionAttribute.g.cs
namespace SomeNamespace
{
    public partial class TemplateTarget
    {
        public IEnumerable<string> Names => names.AsReadOnly(); // Encapsulating List<string> 
        public IEnumerable<string> Addresses => addresses.AsReadOnly(); // Encapsulating List<string> 
        public IEnumerable<string> Books => books.AsReadOnly(); // Encapsulating List<string> 
    }
}