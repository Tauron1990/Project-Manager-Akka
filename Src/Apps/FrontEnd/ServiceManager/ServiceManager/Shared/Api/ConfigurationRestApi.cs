namespace ServiceManager.Shared.Api
{
    public static class ConfigurationRestApi
    {
        private const string BaseUrl = "api/configuration";

        public const string GetBaseConfig = BaseUrl + "/" + nameof(GetBaseConfig);

        public const string GetBaseConfigOptions = BaseUrl + "/" + nameof(GetBaseConfigOptions);

        public const string GlobalConfig = BaseUrl + "/" + nameof(GlobalConfig);

        public static class ModuleName
        {
            public const string Akka = "Akka";

            public const string AkkaRemote = "AkkaRemote";

            public const string AkkaPersistence = "AkkaPersistence";

            public const string AkkaCluster = "AkkaCluster";

            public const string AkkaStreams = "AkkaStreams";
        }
    }
}