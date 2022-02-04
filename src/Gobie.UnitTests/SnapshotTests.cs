namespace Gobie.UnitTests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using VerifyNUnit;

    [TestFixture]
    public class SnapshotTests
    {
        [Test]
        public Task Test()
        {
            var person = new
            {
                Id = "ebced679-45d3-4653-8791-3d969c4a6c",
                Title = "Mr.",
                GivenNames = "Johnnyzzz",
                FamilyName = "Smith",
                Spouse = "Jill",
                Children = new List<string> 
                {
                    "Sam",
                    "Mary"
                },
                Address = new
                {
                    Street = "4 Puddle Lane",
                    Country = "USA"
                }
            };
            return Verifier.Verify(person);
        }
    }
}
