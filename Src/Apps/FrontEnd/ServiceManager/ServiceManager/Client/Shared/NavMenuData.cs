using ServiceManager.Shared.Apps;

namespace ServiceManager.Client.Shared
{
    public sealed record NavMenuData(bool Database, NeedSetupData Apps);
}