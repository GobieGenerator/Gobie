using System;
using System.Linq;

namespace ConsoleClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var auth = new Author();
            auth.AddBooks(new Book { Title = "Spellmonger" });
            auth.AddBooks(new Book { Title = "The Warrior's Apprentice" });

            foreach (var book in auth.Books)
            {
                Console.WriteLine(book.Title);
            }

            //Console.WriteLine($"Author has {auth.StateLog.Count()} log entries");
        }
    }
}
