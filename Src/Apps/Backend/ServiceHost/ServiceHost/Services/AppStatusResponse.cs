using System;
using System.Collections.Immutable;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.Services
{
    public sealed record AppStatusResponse(Guid OpId, ImmutableDictionary<AppName, AppState> Apps);
}