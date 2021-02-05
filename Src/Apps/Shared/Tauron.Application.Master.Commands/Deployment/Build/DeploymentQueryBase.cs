using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build
{
    [PublicAPI]
    public abstract record DeploymentQueryBase<TThis, TResult>
        (string AppName) : ResultCommand<DeploymentApi, TThis, TResult>, IDeploymentQuery
        where TThis : ResultCommand<DeploymentApi, TThis, TResult>
    {
        protected override string Info => AppName;
    }
}