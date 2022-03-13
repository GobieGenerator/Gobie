namespace Gobie.UnitTests;

public class ClassIdentifierTests
{
    [TestCase("a", "b", "global::a.b")]
    [TestCase("", "b", "global::b")]
    public void FullName_IsCorrect(string namespaceName, string className, string fullName)
    {
        var sut = new ClassIdentifier(namespaceName, className);

        Assert.AreEqual(className, sut.ClassName);
        Assert.AreEqual(namespaceName, sut.NamespaceName);
        Assert.AreEqual(fullName, sut.FullName);
    }
}
