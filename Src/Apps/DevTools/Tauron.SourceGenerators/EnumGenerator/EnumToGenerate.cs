using System.Collections.Immutable;

namespace Tauron.SourceGenerators.EnumGenerator;

public record struct EnumToGenerate(string Name, ImmutableArray<string> Values, string ExtensionName, string NamespaceName);