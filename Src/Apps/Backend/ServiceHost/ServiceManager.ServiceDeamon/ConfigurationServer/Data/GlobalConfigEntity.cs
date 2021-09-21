using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceManager.ServiceDeamon.ConfigurationServer.Data
{
    public sealed record GlobalConfigEntity(string Id, GlobalConfig Config)
    {
        public const string EntityId = "E0996549-0E6B-40EB-AD4F-BCB74D27DC3F";

        public GlobalConfigEntity()
            : this(string.Empty, new GlobalConfig(string.Empty, null)) { }
    }
}