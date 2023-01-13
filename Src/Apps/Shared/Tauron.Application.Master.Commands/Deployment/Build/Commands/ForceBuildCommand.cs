using Tauron.Application.AkkaNode.Services.Reporting.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands;

public sealed record ForceBuildCommand(RepositoryName Repository, ProjectName Project) : FileTransferCommand<DeploymentApi, ForceBuildCommand>, IDeploymentCommand
{
    protected override string Info => $"{Repository}.{Project}";
    public AppName AppName => AppName.From("No");
}