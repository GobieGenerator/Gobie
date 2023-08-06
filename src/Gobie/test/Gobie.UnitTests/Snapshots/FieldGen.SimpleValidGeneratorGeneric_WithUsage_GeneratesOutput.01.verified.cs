//HintName: TemplateTarget_EncapsulatedCollectionAttribute.g.cs
namespace SomeNamespace
{
    public partial class TemplateTarget
    {
        public IEnumerable<string> Names => names.AsReadOnly(); // Encapsulating List<string> 
    }
}