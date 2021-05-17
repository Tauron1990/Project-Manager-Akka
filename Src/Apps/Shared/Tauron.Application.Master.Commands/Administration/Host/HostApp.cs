using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    [PublicAPI]
    public sealed record HostApp(string SoftwareName, string Name, string Path, int AppVersion, AppType AppType, string Exe, bool Running);
}