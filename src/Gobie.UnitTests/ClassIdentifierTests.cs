namespace Gobie.UnitTests;

public class ClassIdentifierTests
{
    [TestCase("a", "b", "a.b", "global::a.b")]
    [TestCase("", "b", "b", "global::b")]
    public void FullName_IsCorrect(string namespaceName, string className, string fullName, string globalName)
    {
        var sut = new ClassIdentifier(namespaceName, className);

        Assert.AreEqual(className, sut.ClassName);
        Assert.AreEqual(namespaceName, sut.NamespaceName);
        Assert.AreEqual(fullName, sut.FullName);
        Assert.AreEqual(globalName, sut.GlobalName);
    }
}
