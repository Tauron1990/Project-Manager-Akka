using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build;

[PublicAPI]
public abstract record DeploymentCommandBase<TThis, TResult>(AppName AppName) : ResultCommand<DeploymentApi, TThis, TResult>, IDeploymentCommand
    where TThis : ResultCommand<DeploymentApi, TThis, TResult>
{
    protected override string Info => AppName.Value;
}