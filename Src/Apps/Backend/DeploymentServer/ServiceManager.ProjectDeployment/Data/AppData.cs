using System;
using System.Collections.Immutable;
using System.Linq;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed record AppData(ImmutableList<AppFileInfo> Versions, AppName Id, SimpleVersion Last, DateTime CreationTime, DateTime LastUpdate, RepositoryName Repository, ProjectName ProjectName)
    {
        public static readonly AppData Empty = new();

        public AppData()
            : this(ImmutableList<AppFileInfo>.Empty, AppName.Empty, SimpleVersion.NoVersion, DateTime.MinValue, DateTime.MinValue, RepositoryName.Empty, ProjectName.Empty) { }

        public AppInfo ToInfo()
            => new(
                Id,
                Last,
                LastUpdate,
                CreationTime,
                Repository,
                Versions.Select(fi => new AppBinary(fi.Version, Id, fi.CreationTime, fi.Deleted, fi.Commit, Repository))
                   .ToImmutableList(),
                Deleted: false,
                ProjectName);
    }
}