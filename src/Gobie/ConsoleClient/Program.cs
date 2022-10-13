using Gobie;
using System;
using System.Linq;

namespace ConsoleClient
{
    public sealed class UserDefinedGenerator : Gobie.GobieClassGenerator
    {
        [GobieTemplate]
        private const string EncapsulatedCollection = @"
            public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> {{FieldName : pascal}} => {{FieldName}}.AsReadOnly();

            public void Add{{ FieldName : pascal }}({{FieldGenericType}} s)
            {
                if(s is null)
                {
                    throw new ArgumentNullException(nameof(s));
                }

                {{#CustomValidator}}
                if({{CustomValidator}}(s))
                {
                    {{FieldName}}.Add(s);
                }
                {{/CustomValidator}}

                {{^CustomValidator}}
                    {{FieldName}}.Add(s);
                {{/CustomValidator}}
            }";

        [GobieTemplate]
        private const string EncapsulatedCollection2 = "interpolated const {{EncapsulatedCollection : pascal}} \r\n";
    }

    ////public sealed class UserDefined3Generator : Gobie.GobieClassGenerator
    ////{
    ////    [GobieTemplate]
    ////    private const string EncapsulatedCollection = "public System.Collections.Generic.IEnumerable<{{FieldGenericType}}> \r \n \r\n{{FieldName : BADTAG}} => {{FieldName}}.AsReadOnly();";
    ////}

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
