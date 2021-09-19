using System;
using System.Collections.Immutable;
using System.Linq;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed record AppData(ImmutableList<AppFileInfo> Versions, string Id, int Last, DateTime CreationTime, DateTime LastUpdate, string Repository, string ProjectName)
    {
        public static readonly AppData Empty = new();

        public AppData()
            : this(ImmutableList<AppFileInfo>.Empty, string.Empty, -1, DateTime.MinValue, DateTime.MinValue, string.Empty, string.Empty)
        {
            
        }

        public AppInfo ToInfo()
            => new(Id, Last, LastUpdate, CreationTime, Repository,
                Versions.Select(fi => new AppBinary(fi.Version, Id, fi.CreationTime, fi.Deleted, fi.Commit, Repository))
                        .ToImmutableList(), Deleted: false, ProjectName);
    }
}