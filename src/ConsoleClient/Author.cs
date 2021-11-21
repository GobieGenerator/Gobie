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
        [EncapulatedCollection(TemplateDebug = false, CustomValidator = nameof(ValidateBooks))]
        private List<string> books = new();

        [EncapulatedCollection()]
        private List<string> publishers = new();

        public bool ValidateBooks(string a)
        {
            return true;
        }
    }
}
