namespace ConsoleClient
{
    using Models;
    using System;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var auth = new Author();
            auth.AddBooks("Spellmonger");
            auth.AddBooks("The Warrior's Apprentace");

            foreach (var book in auth.Books)
            {
                Console.WriteLine(book);
            }
        }
    }
}
