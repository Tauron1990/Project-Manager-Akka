using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.Administration.Host
{
    public sealed record HostAppsResponse(ImmutableList<HostApp> Apps, bool Success) : OperationResponse(Success)
    {
        public HostAppsResponse()  
            : this(ImmutableList<HostApp>.Empty, false)
        { }
    }
}