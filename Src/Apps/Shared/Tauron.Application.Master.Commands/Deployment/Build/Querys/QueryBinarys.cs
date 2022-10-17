using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build.Querys;

[PublicAPI]
public sealed record QueryBinarys(AppName AppName, SimpleVersion AppVersion) : FileTransferCommand<DeploymentApi, QueryBinarys>, IDeploymentQuery
{
    protected override string Info => AppName.Value;
}