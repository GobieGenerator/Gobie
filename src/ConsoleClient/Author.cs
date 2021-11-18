using Gobie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient.Models
{
    public partial class Author
    {
        [CustomFieldGenerator]
        private List<string> books = new();

        [CustomFieldGenerator]
        private List<string> books2 = new();

        private List<string> sdf = new();
    }
}
