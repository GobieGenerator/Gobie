using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient
{
    public class Author
    {
        private const string EncapsulationTemplate = @"

public class {{Property}}Generated
{
    private System.Collections.Generic.List<string> {{ field }} = new();

    public System.Collections.Generic.IEnumerable<string> {{ Property }} => {{ field }}.AsReadOnly();
}
";

        [Gobie.GobieFieldGenerator(EncapsulationTemplate)]
        private List<string> books = new();

        private List<string> books2 = new();

        [Gobie.GobieFieldGenerator(EncapsulationTemplate)]
        private List<string> sdf = new();
    }
}
