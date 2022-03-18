using System.Threading.Tasks;

namespace Tauron.SourceGenerators.Tests.EnumGenerator;

using VerifyXunit;
using Xunit;

[UsesVerify] // 👈 Adds hooks for Verify into XUnit
public class EnumGeneratorSnapshotTests
{
    [Fact]
    public Task GeneratesEnumExtensionsCorrectly()
    {
        // The source code to test
        const string source = @"
using Tauron.SourceGenerators.EnumGenerators;

[EnumExtensions]
public enum Colour
{
    Red = 0,
    Blue = 1,
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
    
    [Fact]
    public Task GeneratesEnumExtensionsWithCustomNamesCorrectly()
    {
        // The source code to test
        const string source = "using Tauron.SourceGenerators.EnumGenerators;[EnumExtensions(ExtensionClassName = \"TestName\", ExtensionNamespaceName = \"TestNamespace\")] public enum Colour { Red = 0, Blue = 1, }";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}