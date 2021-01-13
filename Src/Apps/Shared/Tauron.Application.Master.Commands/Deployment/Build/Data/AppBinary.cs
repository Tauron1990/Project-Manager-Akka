using System;
using JetBrains.Annotations;

namespace Tauron.Application.Master.Commands.Deployment.Build.Data
{
    [PublicAPI]
    public sealed record AppBinary(int FileVersion, string AppName, DateTime CreationTime, bool Deleted, string Commit, string Repository);
}