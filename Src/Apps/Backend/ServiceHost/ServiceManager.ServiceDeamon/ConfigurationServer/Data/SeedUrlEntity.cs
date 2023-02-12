using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Data
{
    public sealed record SeedUrlEntity(string Id, SeedUrl Url)
    {
        public SeedUrlEntity()
            : this(string.Empty, new SeedUrl(string.Empty, null)) { }
    }
}