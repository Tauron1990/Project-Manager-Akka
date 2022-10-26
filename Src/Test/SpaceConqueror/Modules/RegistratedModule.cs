using System.Collections.Immutable;

namespace SpaceConqueror.Modules;

public sealed record RegistratedModule(string Name, Version Version);

public sealed record GamePackage(string Name, ImmutableList<RegistratedModule> Modules);