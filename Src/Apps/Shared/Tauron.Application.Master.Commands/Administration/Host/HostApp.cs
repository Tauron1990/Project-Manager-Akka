using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    [PublicAPI]
    public sealed record HostApp(string Name, string Path, int AppVersion, AppType AppType, bool SupressWindow,
        string Exe, bool Running);
}