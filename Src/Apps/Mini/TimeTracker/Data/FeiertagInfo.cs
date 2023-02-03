using System.Collections.Immutable;

namespace TimeTracker.Data;

public sealed record FeiertagInfo(string Name, ImmutableList<Laender> Laender);