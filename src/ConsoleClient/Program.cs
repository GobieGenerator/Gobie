namespace ConsoleClient
{
    using System;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var auth = new Author();

            auth.AddBooks("My first book");
            auth.AddBooks("Another");

            foreach (var book in auth.Books)
            {
                Console.WriteLine(book);
            }

            foreach (var bl in auth.BooksLengths)
            {
                Console.WriteLine(bl);
            }
        }
    }
}
