namespace Gobie
{
 

    /// <summary>
    /// Attribute produced by gobie. Defined in <see cref="EncapulatedCollectionGenerator"/>.
    /// </summary>
    public sealed class EncapulatedCollectionAttribute : GobieFieldGeneratorAttribute
    {
        public string CustomValidator { get; set; } = null;
    }
}
