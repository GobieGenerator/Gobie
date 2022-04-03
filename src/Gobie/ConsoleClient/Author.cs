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
        private const string KeyString = "public string Name { get; set; } = \"{{Num1}} of {{Num2}}\";";

        [GobieTemplate]
        private const string IdString = @"public int IdentNum { get; set; } = {{Num1}};

        private static int Temp             => 3;
private static int Temp1             => 3;
private static int Temp2            => 3;
private static int Temp3             => 3;
";

        [Required(5)]
        public int Num1 { get; set; }

        [Required(11)]
        public int Num2 { get; set; } = 42;

        public string OptionalString { get; set; } = "favorite quote: \"Hello from the magic tavern\"";
    }

    [PkGen]
    [NameProperty(25)]
    public partial class GenTarget
    { }

    [PkGen]
    public partial class GenTarget2
    { }
}
