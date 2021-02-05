using Tauron.Application.AkkaNode.Services.Commands;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed record ForceBuildCommand
        (string Repository, string Project) : FileTransferCommand<DeploymentApi, ForceBuildCommand>, IDeploymentCommand
    {
        protected override string Info => $"{Repository}.{Project}";
        public string AppName => "No";
    }
}