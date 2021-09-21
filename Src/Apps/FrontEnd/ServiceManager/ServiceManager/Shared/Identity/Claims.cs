namespace ServiceManager.Shared.Identity
{
    public static class Claims
    {
        public const string AppIpClaim = nameof(AppIpClaim);

        public const string ClusterConnectionClaim = nameof(ClusterConnectionClaim);

        public const string ClusterNodeClaim = nameof(ClusterNodeClaim);

        public const string ConfigurationClaim = nameof(ConfigurationClaim);

        public const string DatabaseClaim = nameof(DatabaseClaim);

        public const string ServerInfoClaim = nameof(ServerInfoClaim);

        public const string UserManagmaentClaim = nameof(UserManagmaentClaim);

        public const string AppMenegmentClaim = nameof(AppMenegmentClaim);

        public static readonly string[] AllClaims =
        {
            AppIpClaim,
            ClusterConnectionClaim,
            ClusterNodeClaim,
            ConfigurationClaim,
            DatabaseClaim,
            ServerInfoClaim,
            UserManagmaentClaim,
            AppMenegmentClaim
        };
    }
}