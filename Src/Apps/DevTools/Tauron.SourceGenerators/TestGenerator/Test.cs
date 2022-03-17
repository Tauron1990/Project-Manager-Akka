using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tauron.SourceGenerators.TestGenerator;

[Generator(LanguageNames.CSharp)]
public class Test : IIncrementalGenerator
{
    private const string TargetAttribute = "Tauron.SourceGenerators.Attributes.TestAttribute"
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var filter = context.SyntaxProvider
           .CreateSyntaxProvider(static (node, _) => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 }, SelectClass)
           .Where(v => v is not null);
        
        
        var provider = context.SyntaxProvider
           .CreateSyntaxProvider(
                static (node, _) => node.IsKind(SyntaxKind.ClassDeclaration)
                          && ((ClassDeclarationSyntax)node).AttributeLists.Any(al => al.Attributes.Any(a => a.Name.Span.ToString().StartsWith("GenerateTest"))),
                static (node, _) => node.Node as ClassDeclarationSyntax)
           .Where(static s => s is not null);
        
        context.RegisterSourceOutput(provider, Execute!);
    }

    private static (SemanticModel Model, ClassDeclarationSyntax Syntax)? SelectClass(GeneratorSyntaxContext context, CancellationToken token)
    {
        if (context.Node is not ClassDeclarationSyntax classSyntax)
            return null;

        if (classSyntax.AttributeLists
           .TakeWhile(_ => !token.IsCancellationRequested)
           .Any(attributeList => attributeList.Attributes
                   .TakeWhile(_ => !token.IsCancellationRequested)
                   .Select(attribute => context.SemanticModel.GetDeclaredSymbol(attribute, token))
                   .Where(symbol => symbol != null)
                   .Any(symbol => symbol?.ContainingType.ToDisplayString() == TargetAttribute)))
        {
            return (context.SemanticModel, classSyntax);
        }

        return null;
    }
    
    private void Execute(SourceProductionContext context, ClassDeclarationSyntax syntax)
    {
        context.AddSource(syntax.Identifier.Text + ".g.cs", "");
    }
}