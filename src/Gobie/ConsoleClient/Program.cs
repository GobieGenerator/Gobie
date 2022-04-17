namespace ConsoleClient
{
    using Models;
    using System;
    using System.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var auth = new Author();
            auth.AddBooks("Spellmonger");
            auth.AddBooks("The Warrior's Apprentice");

            foreach (var book in auth.Books)
            {
                Console.WriteLine(book);
            }

            Console.WriteLine($"Author has {auth.Logs.Count()} log entry");
        }
    }
}
