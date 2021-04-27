using System;

namespace ServiceManager.ProjectDeployment.Data
{
    public sealed record AppFileInfo(string Id, string File, int Version, DateTime CreationTime, bool Deleted, string Commit);
}