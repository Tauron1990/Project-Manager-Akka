namespace ServiceManager.Client.Shared.BaseComponents
{
    public sealed record AppData(bool IsConnected, bool IsSelf, bool IsDatabaseReady);
}