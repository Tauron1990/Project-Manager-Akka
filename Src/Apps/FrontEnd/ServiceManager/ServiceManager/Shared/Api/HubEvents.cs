namespace ServiceManager.Shared.Api
{
    public static class HubEvents
    {
        public const string PropertyChanged = nameof(PropertyChanged);

        public const string RestartServer = nameof(RestartServer);

        public const string AppsConfigChanged = nameof(AppsConfigChanged);
    }
}