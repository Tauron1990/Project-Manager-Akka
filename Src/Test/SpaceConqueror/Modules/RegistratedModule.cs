using System.Collections.Immutable;

namespace SpaceConqueror.Modules;

public sealed record RegistratedModule(string Name, Version Version);

public sealed record RegistratedModuleCategory(string Name, ImmutableList<RegistratedModule> Modules);