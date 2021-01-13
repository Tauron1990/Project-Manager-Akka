using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed record CreateAppCommand(string AppName, string TargetRepo, string ProjectName) : DeploymentCommandBase<CreateAppCommand, AppInfo>(AppName);
}