namespace Gobie.UnitTests
{
    using NetEscapades.EnumGenerators.Tests;
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
        public Task GeneratesEnumExtensionsCorrectly()
        {
            // The source code to test
            var source = @"
using NetEscapades.EnumGenerators;

[EnumExtensions]
public enum Colour
{
Red = 0,
Blue = 1,
}";
            
            // Pass the source code to our helper and snapshot test the output
            return TestHelper.Verify(source);
        }
    }
}
