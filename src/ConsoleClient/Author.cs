using Gobie;
using System.Collections.Generic;

namespace ConsoleClient.Models
{
    public partial class Author
    {
        [EncapulatedCollection(CustomValidator = nameof(ValidateBooks))]
        private List<string> books = new();

        [EncapulatedCollection()]
        private List<string> publishers = new();

        public bool ValidateBooks(string a)
        {
            return true;
        }
    }
}
