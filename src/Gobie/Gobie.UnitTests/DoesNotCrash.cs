namespace Gobie.UnitTests
{
    public class DoesNotCrash
    {
        [Test]
        public void Crash()
        {
            var files = Directory.GetFiles(@"../../../Assets/GeneratorCrashes", "*.cs");
            foreach (var file in files)
            {
                var code = File.ReadAllText(file);
                Assert.DoesNotThrow(() => TestHelper.RunGeneration(code));
            }
        }
    }
}
