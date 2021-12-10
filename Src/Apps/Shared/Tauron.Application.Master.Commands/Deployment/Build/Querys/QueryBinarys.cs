using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys;

[PublicAPI]
public sealed record QueryBinarys(string AppName, int AppVersion = -1) : FileTransferCommand<DeploymentApi, QueryBinarys>, IDeploymentQuery
{
    protected override string Info => AppName;
}