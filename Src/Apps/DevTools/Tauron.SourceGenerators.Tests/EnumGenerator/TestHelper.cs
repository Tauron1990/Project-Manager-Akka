using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VerifyXunit;

namespace Tauron.SourceGenerators.Tests.EnumGenerator;


public static class TestHelper
{
    public static Task Verify(string source)
    {
        // Parse the provided string into a C# syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references for assemblies we require
        // We could add multiple references if required
        var references = new[]
                         {
                             MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                         };
        
        // Create a Roslyn compilation for the syntax tree.
        var compilation = CSharpCompilation.Create(
            "Tests",
            new[] { syntaxTree },
            references);


        // Create an instance of our EnumGenerator incremental source generator
        var generator = new SourceGenerators.EnumGenerator.Generator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver);
    }
}