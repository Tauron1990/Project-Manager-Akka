namespace ServiceManager.Shared.Api
{
    public static class ConfigurationRestApi
    {
        private const string BaseUrl = "api/configuration";

        public const string GetConfigFile = BaseUrl + "/" + nameof(GetConfigFile);

        public const string GetConfigFileOptions = BaseUrl + "/" + nameof(GetConfigFileOptions);

        public const string GlobalConfig = BaseUrl + "/" + nameof(GlobalConfig);

        public const string GetBaseConfig = BaseUrl + "/" + nameof(GetBaseConfig);

        public const string ServerConfiguration = BaseUrl + "/" + nameof(ServerConfiguration);

        public const string GetAppConfigList = BaseUrl + "/" + nameof(GetAppConfigList);

        public const string DeleteSpecificConfig = BaseUrl + "/" + nameof(DeleteSpecificConfig);

        public const string UpdateSpecificConfigiration = BaseUrl + "/" + nameof(UpdateSpecificConfigiration);
        
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