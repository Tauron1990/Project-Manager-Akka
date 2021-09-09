namespace ServiceManager.Shared.Api
{
    public enum ConfigOpensElement
    {
        Akka,
        AkkaRemote,
        AkkaPersistence,
        AkkaCluster,
        AkkaStreams
    }
    
    public static class ControllerName
    {
        public const string UserManagment = "api/usermanagement";
        
        public const string AppConfiguration = "api/configuration";
        
        public const string DatabaseConfigApiBase = "api/databaseconfig";
        
        public const string ClusterNoteTracking = "api/ClusterNoteTracking";

        public const string ClusterConnectionTracker = "api/ClusterConnectionTracker";

        public const string ServerInfo = "api/ServerInfo";

        public const string AppIpManager = "api/AppIpManager";

        public const string AppManagment = "api/appmanagment";
    }
}