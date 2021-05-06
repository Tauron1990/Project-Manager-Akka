using System.Collections.Immutable;

namespace ServiceHost.Client.Shared.ConfigurationServer.Data
{
    public sealed record SeedUrl(string Url, string? Info);

    public sealed record SeedUrls(ImmutableList<SeedUrl> Urls);
}