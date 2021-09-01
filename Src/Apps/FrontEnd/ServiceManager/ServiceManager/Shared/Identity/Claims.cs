using System.Collections.Generic;

namespace ServiceManager.Shared.Identity
{
    public static class Claims
    {
        public static IEnumerable<string> AllClaims = new[]
                                               {
                                                   AppIpClaim,
                                                   ClusterConnectionClaim,
                                                   ClusterNodeClaim,
                                                   ConfigurationClaim,
                                                   DatabaseClaim,
                                                   ServerInfoClaim,
                                                   UserManagmaent
                                               };

        public const string AppIpClaim = nameof(AppIpClaim);

        public const string ClusterConnectionClaim = nameof(ClusterConnectionClaim);

        public const string ClusterNodeClaim = nameof(ClusterNodeClaim);

        public const string ConfigurationClaim = nameof(ConfigurationClaim);

        public const string DatabaseClaim = nameof(DatabaseClaim);

        public const string ServerInfoClaim = nameof(ServerInfoClaim);

        public const string UserManagmaent = nameof(UserManagmaent);
    }
}