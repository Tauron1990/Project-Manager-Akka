using System;
using System.Collections.Immutable;

namespace ServiceHost.Services
{
    public sealed record AppStatusResponse(Guid OpId, ImmutableDictionary<string, bool> Apps);
}