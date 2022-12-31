using System;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data;

[PublicAPI]
public sealed record AppBinary(SimpleVersion FileVersion, AppName AppName, DateTime CreationTime, bool Deleted, RepositoryCommit Commit, RepositoryName Repository)
{
    public static readonly AppBinary Empty = new(SimpleVersion.NoVersion, AppName.Empty, DateTime.MinValue, Deleted: true, RepositoryCommit.Empty, RepositoryName.Empty);
}