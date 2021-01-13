using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands
{
    public sealed record DeleteAppCommand(string AppName) : DeploymentCommandBase<DeleteAppCommand, AppInfo>(AppName);
}