using System.Collections.Immutable;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data;

public sealed record AppList(ImmutableList<AppInfo> Apps)
{
    //public static readonly AppList Empty = new(ImmutableList<AppInfo>.Empty);
}