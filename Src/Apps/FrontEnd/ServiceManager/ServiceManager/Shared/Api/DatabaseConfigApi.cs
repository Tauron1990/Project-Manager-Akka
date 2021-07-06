namespace ServiceManager.Shared.Api
{
    public static class DatabaseConfigApi
    {
        public const string DatabaseConfigApiBase = "api/databaseconfig";

        public const string IsReady = DatabaseConfigApiBase + "/" + nameof(IsReady);

        public const string FetchUrl = DatabaseConfigApiBase + "/" + nameof(FetchUrl);
    }
}