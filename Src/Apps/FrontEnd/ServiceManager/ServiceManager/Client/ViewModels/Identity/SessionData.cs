using System;

namespace ServiceManager.Client.ViewModels.Identity
{
    public sealed record SessionData(string SessionId, DateTime Timeout, bool IsLogedIn);
}