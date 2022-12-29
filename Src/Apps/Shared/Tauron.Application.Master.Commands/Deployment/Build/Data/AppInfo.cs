using System;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data;

[PublicAPI]
public sealed record AppInfo(
    AppName Name, SimpleVersion LastVersion, DateTime UpdateDate, DateTime CreationTime, RepositoryName Repository,
    ImmutableList<AppBinary> Binaries, bool Deleted, ProjectName ProjectName)
{
    public static readonly AppInfo Empty = new(
        AppName.Empty,
        SimpleVersion.NoVersion,
        DateTime.MinValue,
        DateTime.MinValue,
        RepositoryName.Empty,
        ImmutableList<AppBinary>.Empty,
        true,
        ProjectName.Empty);

    public AppInfo MarkDeleted() => this with { Deleted = true };
}