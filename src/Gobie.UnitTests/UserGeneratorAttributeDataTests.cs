namespace Gobie.UnitTests;

using Microsoft.CodeAnalysis.CSharp.Syntax;

public class UserGeneratorAttributeDataTests
{
    private static ClassDeclarationSyntax TrivialCDS => SyntaxFactory.ClassDeclaration("Trivial");

    [TestCase("MyNamespace", "PrimaryKey", "PrimaryKeyAttribute")]
    [TestCase("MyNamespace", "PrimaryKeyGenerator", "PrimaryKeyAttribute")]
    public void Test(string namespaceName, string className, string attributeName)
    {
        var sut = new UserGeneratorAttributeData(new ClassIdentifier(namespaceName, className), TrivialCDS);

        Assert.AreEqual(namespaceName, sut.DefinitionIdentifier.NamespaceName);
        Assert.AreEqual(className, sut.DefinitionIdentifier.ClassName);
        Assert.AreEqual(namespaceName, sut.AttributeIdentifier.NamespaceName);
        Assert.AreEqual(attributeName, sut.AttributeIdentifier.ClassName);
    }
}
