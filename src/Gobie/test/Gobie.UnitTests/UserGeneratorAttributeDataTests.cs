namespace Gobie.UnitTests;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public class UserGeneratorAttributeDataTests
{
    private static ClassDeclarationSyntax TrivialCDS => SyntaxFactory.ClassDeclaration("Trivial");

    [TestCase("MyNamespace", "PrimaryKey", "PrimaryKeyAttribute")]
    [TestCase("MyNamespace", "PrimaryKeyGenerator", "PrimaryKeyAttribute")]
    public void CtorTests(string namespaceName, string className, string attributeName)
    {
        var sut = new UserGeneratorAttributeData(new ClassIdentifier(namespaceName, className), TrivialCDS, "");

        Assert.AreEqual(namespaceName, sut.DefinitionIdentifier.NamespaceName);
        Assert.AreEqual(className, sut.DefinitionIdentifier.ClassName);
        Assert.AreEqual(namespaceName, sut.AttributeIdentifier.NamespaceName);
        Assert.AreEqual(attributeName, sut.AttributeIdentifier.ClassName);
    }

    [TestCase("MyNamespace", "PrimaryKey", "PkGen", null, "MyNamespace", "PkGenAttribute")]
    [TestCase("MyNamespace", "PrimaryKey", "PkGen", "CustomNamespace", "CustomNamespace", "PkGenAttribute")]
    [TestCase("MyNamespace", "PrimaryKey", "PkGenAttribute", null, "MyNamespace", "PkGenAttribute")]
    [TestCase("MyNamespace", "PrimaryKey", "PkGenAttribute", "CustomNamespace", "CustomNamespace", "PkGenAttribute")]
    public void WithNameTests(string namespaceName, string className, string withName, string? withNamespace, string attributeNamespace, string attributeName)
    {
        var sut = new UserGeneratorAttributeData(new ClassIdentifier(namespaceName, className), TrivialCDS, "");
        sut = sut.WithName(withName, withNamespace);

        Assert.AreEqual(namespaceName, sut.DefinitionIdentifier.NamespaceName);
        Assert.AreEqual(className, sut.DefinitionIdentifier.ClassName);
        Assert.AreEqual(attributeNamespace, sut.AttributeIdentifier.NamespaceName);
        Assert.AreEqual(attributeName, sut.AttributeIdentifier.ClassName);
    }
}
