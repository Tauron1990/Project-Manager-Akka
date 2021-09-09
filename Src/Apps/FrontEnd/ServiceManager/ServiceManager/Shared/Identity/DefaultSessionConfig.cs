using System;

namespace ServiceManager.Shared.Identity
{
    public static class DefaultSessionConfig
    {
        public const string Name = "FusionAuth.SessionId";

        public static readonly TimeSpan Timeout = TimeSpan.FromDays(28);
    }
}