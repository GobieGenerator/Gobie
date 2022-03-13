using Gobie;
using System.Collections.Generic;

namespace ConsoleClient.Models
{
    [GobieGeneratorName("PkGen")]
    public sealed class PrimaryKeyGenerator : GobieFieldGenerator
    {
        [GobieTemplate]
        private const string KeyString = "public int Id { get; set; } // This is a key";
    }

    public sealed class NamePropertyGenerator : GobieFieldGenerator
    {
        [GobieTemplate]
        private const string KeyString = "public string Name { get; set; }";
    }

    [PkGen]
    [NameProperty]
    public partial class GenTarget
    { }

    [PkGen]
    public partial class GenTarget2
    { }
}
