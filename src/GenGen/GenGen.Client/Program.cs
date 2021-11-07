namespace GenGen.Client
{
    using System;

    internal class Program
    {
        private const string m = @"{{#repo}}
  <b>{{name}}</b>
{{/repo}}
{{^repo}}
  No repos :(
{{/repo}}";

        private static void Main(string[] args)
        {
            Console.WriteLine(m);
        }
    }
}
