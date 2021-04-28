using System;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed record AppFileInfo(string Id, int Version, DateTime CreationTime, bool Deleted, string Commit);
}