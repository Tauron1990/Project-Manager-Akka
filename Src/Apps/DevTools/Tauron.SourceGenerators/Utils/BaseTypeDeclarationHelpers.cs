using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tauron.SourceGenerators.Utils;

public static class BaseTypeDeclarationHelpers
{
    public static bool MatchTypeAndHasAttributes<TTarget>(this SyntaxNode node, CancellationToken token)
        where TTarget : BaseTypeDeclarationSyntax
    // ReSharper disable once RedundantTypeCheckInPattern
        => node is TTarget { AttributeLists.Count: > 0 };

    public static Func<GeneratorSyntaxContext, CancellationToken, TTarget?> GetSemanticTargetForGeneration<TTarget>(Func<INamedTypeSymbol, bool> checkAttribute)
        where TTarget : BaseTypeDeclarationSyntax
        => (context, token) =>
           {
               if (context.Node is not TTarget enumDeclarationSyntax)
                   return default;

               // loop through all the attributes on the method
               foreach (var attributeSyntax in enumDeclarationSyntax.AttributeLists.SelectMany(attributeListSyntax => attributeListSyntax.Attributes))
               {
                   token.ThrowIfCancellationRequested();
                   
                   if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                       // weird, we couldn't get the symbol, ignore it
                       continue;

                   // Is the attribute the [EnumExtensions] attribute?
                   if (checkAttribute(attributeSymbol.ContainingType))
                       // return the enum
                       return enumDeclarationSyntax;
               }

               // we didn't find the attribute we were looking for
               return default;
           };
}