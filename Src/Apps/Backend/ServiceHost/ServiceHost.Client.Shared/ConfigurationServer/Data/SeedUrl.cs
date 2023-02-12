using System.Collections.Immutable;

namespace ServiceHost.ClientApp.Shared.ConfigurationServer.Data;

public sealed record SeedUrl(string Url, string? Info);

public sealed record SeedUrls(ImmutableList<SeedUrl> Urls);