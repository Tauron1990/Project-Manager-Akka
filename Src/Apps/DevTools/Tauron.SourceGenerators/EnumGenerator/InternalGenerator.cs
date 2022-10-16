using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Tauron.SourceGenerators.Utils;

namespace Tauron.SourceGenerators.EnumGenerator;

[Generator(LanguageNames.CSharp)]
public class InternalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attributeData = AttributeHelpers.GenerateAttribute(
            AttributeTargets.Enum,
            EnumGenerationHelper.Namespace,
            EnumGenerationHelper.TypeName,
            ImmutableDictionary<string, string>.Empty
               .Add("ExtensionClassName", "string")
               .Add("ExtensionNamespaceName", "string")
        );
        
        // Add the marker attribute to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                                                     "EnumExtensionsAttribute.g.cs", 
                                                     SourceText.From(attributeData(), Encoding.UTF8)));

        // Do a simple filter for enums
        IncrementalValuesProvider<EnumDeclarationSyntax> enumDeclarations = context.SyntaxProvider
           .CreateSyntaxProvider(
                BaseTypeDeclarationHelpers.MatchTypeAndHasAttributes<EnumDeclarationSyntax>, // select enums with attributes
                BaseTypeDeclarationHelpers.GetSemanticTargetForGeneration<EnumDeclarationSyntax>(s => s.ToDisplayString() == EnumGenerationHelper.FullName)) // sect the enum with the [EnumExtensions] attribute
           .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

        // Combine the selected enums with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<EnumDeclarationSyntax>)> compilationAndEnums
            = context.CompilationProvider.Combine(enumDeclarations.Collect());

        // Generate the source using the compilation and enums
        context.RegisterSourceOutput(compilationAndEnums,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    
    
    private static void Execute(Compilation compilation, ImmutableArray<EnumDeclarationSyntax> enums, SourceProductionContext context)
    {
        if (enums.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
        var distinctEnums = enums.Distinct();

        // Convert each EnumDeclarationSyntax to an EnumToGenerate
        var enumsToGenerate = GetTypesToGenerate(compilation, distinctEnums, context.CancellationToken);

        // If there were errors in the EnumDeclarationSyntax, we won't create an
        // EnumToGenerate for it, so make sure we have something to generate
        if (enumsToGenerate.Length <= 0) return;

        var groups = enumsToGenerate.GroupBy(g => g.NamespaceName);

        // generate the source code and add it to the output
        foreach (var namespaceGroup in groups)
        {
            string result = EnumGenerationHelper.GenerateExtensionClass(namespaceGroup, namespaceGroup.Key);
            context.AddSource(namespaceGroup.Key + ".g.cs", result);
        }
    }
    
    private static ImmutableArray<EnumToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<EnumDeclarationSyntax> enums, CancellationToken ct)
    {
        // Create a list to hold our output
        var enumsToGenerate = ImmutableArray<EnumToGenerate>.Empty;
        // Get the semantic representation of our marker attribute 
        INamedTypeSymbol? enumAttribute = compilation.GetTypeByMetadataName(EnumGenerationHelper.FullName);

        if (enumAttribute == null)
        {
            // If this is null, the compilation couldn't find the marker attribute type
            // which suggests there's something very wrong! Bail out..
            return enumsToGenerate;
        }

        foreach (var enumDeclarationSyntax in enums)
        {
            // stop if we're asked to
            ct.ThrowIfCancellationRequested();

            // Get the semantic representation of the enum syntax
            var semanticModel = compilation.GetSemanticModel(enumDeclarationSyntax.SyntaxTree);
            if (ModelExtensions.GetDeclaredSymbol(semanticModel, enumDeclarationSyntax) is not INamedTypeSymbol enumSymbol)
            {
                // something went wrong, bail out
                continue;
            }

            // Get the full type name of the enum e.g. Colour, 
            // or OuterClass<T>.Colour if it was nested in a generic type (for example)
            var enumName = enumSymbol.ToString();

            // Get all the members in the enum
            var enumMembers = enumSymbol.GetMembers();
            var members = enumMembers
               .Where(member => member is IFieldSymbol { ConstantValue: { } })
               .Aggregate(ImmutableArray<string>.Empty, (current, member) => current.Add(member.Name));

            // Get all the fields from the enum, and add their name to the list

            // Create an EnumToGenerate for use in the generation phase
            var (className, namespaceName) = ExtractDataInfo(compilation, enumDeclarationSyntax, enumAttribute);
            enumsToGenerate = enumsToGenerate.Add(new EnumToGenerate(enumName, members, className, namespaceName));
        }

        return enumsToGenerate;
    }

    // ReSharper disable once CognitiveComplexity
    private static (string ClassName, string NamespaceName) ExtractDataInfo(Compilation compilation, BaseTypeDeclarationSyntax syntaxNode, ISymbol symbol)
    {
        // Get the semantic model of the enum symbol
        SemanticModel semanticModel = compilation.GetSemanticModel(syntaxNode.SyntaxTree);
        ISymbol? enumSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, syntaxNode);

        // Set the default extension name
        var extensionName = "EnumExtensions";
        string namespaceName = GetNamespace(syntaxNode);

        if (string.IsNullOrWhiteSpace(namespaceName))
            namespaceName = EnumGenerationHelper.Namespace;

        if (enumSymbol is null) return (extensionName, namespaceName);

        // Loop through all of the attributes on the enum
        foreach (AttributeData? attributeData in enumSymbol.GetAttributes()
                    .Where(attributeData => symbol.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default)))
        {
            // This is the attribute, check all of the named arguments
            foreach (var namedArgument in attributeData.NamedArguments)
            {
                switch (namedArgument.Key)
                {
                    // Is this the ExtensionClassName argument?
                    case "ExtensionClassName" when namedArgument.Value.Value?.ToString() is { } className:
                        extensionName = className;
                        break;
                    case "ExtensionNamespaceName" when namedArgument.Value.Value?.ToString() is { } namespaceValue:
                        namespaceName = namespaceValue;
                        break;
                }
            }

            break;
        }

        return (extensionName, namespaceName);
    }
    
    // determine the namespace the class/enum/struct is declared in, if any
    private static string GetNamespace(SyntaxNode syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        var nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        var potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
            && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is not BaseNamespaceDeclarationSyntax namespaceParent) return nameSpace;

        // We have a namespace. Use that as the type
        nameSpace = namespaceParent.Name.ToString();

        // Keep moving "out" of the namespace declarations until we 
        // run out of nested namespace declarations
        while (true)
        {
            if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
            {
                break;
            }

            // Add the outer namespace as a prefix to the final namespace
            nameSpace = $"{namespaceParent.Name}.{nameSpace}";
            namespaceParent = parent;
        }

        // return the final namespace
        return nameSpace;
    }

    // ReSharper disable UnusedMember.Local
    private static ParentClass? GetParentClasses(SyntaxNode typeSyntax)
        
    {
        // Try and get the parent syntax. If it isn't a type like class/struct, this will be null
        var parentSyntax = typeSyntax.Parent as TypeDeclarationSyntax;
        ParentClass? parentClassInfo = null;

        // Keep looping while we're in a supported nested type
        while (parentSyntax != null && IsAllowedKind(parentSyntax.Kind()))
        {
            // Record the parent type keyword (class/struct etc), name, and constraints
            parentClassInfo = new ParentClass(
                parentSyntax.Keyword.ValueText,
                $"{parentSyntax.Identifier.ToString()}{parentSyntax.TypeParameterList}",
                parentSyntax.ConstraintClauses.ToString(),
                parentClassInfo); // set the child link (null initially)

            // Move to the next outer type
            parentSyntax = (parentSyntax.Parent as TypeDeclarationSyntax);
        }

        // return a link to the outermost parent type
        return parentClassInfo;

    }

    // We can only be nested in class/struct/record
    private static bool IsAllowedKind(SyntaxKind kind) =>
        kind is SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or SyntaxKind.RecordDeclaration;
    
    public static string GetResource(string nameSpace, ParentClass? parentClass)
    {
        var sb = new StringBuilder();
        var parentsCount = 0;
        
        // If we don't have a namespace, generate the code in the "default"
        // namespace, either global:: or a different <RootNamespace>
        var hasNamespace = !string.IsNullOrEmpty(nameSpace);
        if (hasNamespace)
        {
            // We could use a file-scoped namespace here which would be a little impler, 
            // but that requires C# 10, which might not be available. 
            // Depends what you want to support!
            sb
               .Append("namespace ")
               .Append(nameSpace)
               .AppendLine(@"
    {");
        }

        // Loop through the full parent type hiearchy, starting with the outermost
        while (parentClass is not null)
        {
            sb
               .Append("    partial ")
               .Append(parentClass.Keyword) // e.g. class/struct/record
               .Append(' ')
               .Append(parentClass.Name) // e.g. Outer/Generic<T>
               .Append(' ')
               .Append(parentClass.Constraints) // e.g. where T: new()
               .AppendLine(@"
        {");
            parentsCount++; // keep track of how many layers deep we are
            parentClass = parentClass.Child; // repeat with the next child
        }

        // Write the actual target generation code here. Not shown for brevity
        sb.AppendLine(@"public partial readonly struct TestId
    {
    }");

        // We need to "close" each of the parent types, so write
        // the required number of '}'
        for (var i = 0; i < parentsCount; i++)
        {
            sb.AppendLine(@"    }");
        }

        // Close the namespace, if we had one
        if (hasNamespace)
        {
            sb.Append('}').AppendLine();
        }

        return sb.ToString();
    }
}
// ReSharper restore UnusedMember.Local