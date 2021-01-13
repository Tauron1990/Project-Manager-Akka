using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record HostAppsResponse(ImmutableList<HostApp> Apps);
}