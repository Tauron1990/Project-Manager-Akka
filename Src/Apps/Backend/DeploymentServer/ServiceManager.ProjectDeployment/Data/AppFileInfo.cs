using System;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed record AppFileInfo(string Id, SimpleVersion Version, DateTime CreationTime, bool Deleted, RepositoryCommit Commit);
}