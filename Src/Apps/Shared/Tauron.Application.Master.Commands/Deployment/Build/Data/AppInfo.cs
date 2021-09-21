using System;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    [PublicAPI]
    public sealed record AppInfo(string Name, int LastVersion, DateTime UpdateDate, DateTime CreationTime, string Repository, ImmutableList<AppBinary> Binaries, bool Deleted, string ProjectName)
    {
        public static readonly AppInfo Empty = new(string.Empty, -1, DateTime.MinValue, DateTime.MinValue, string.Empty, ImmutableList<AppBinary>.Empty, Deleted: true, string.Empty);
        public AppInfo MarkDeleted() => this with { Deleted = true };
    }
}