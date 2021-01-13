using JetBrains.Annotations;
using Tauron.Application.AkkNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys
{
    [PublicAPI]
    public sealed record QueryBinarys(string AppName, int AppVersion = -1) : FileTransferCommand<DeploymentApi, QueryBinarys>
    {
        protected override string Info => AppName;
    }
}