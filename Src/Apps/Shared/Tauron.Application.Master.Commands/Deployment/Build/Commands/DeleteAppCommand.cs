using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands;

public sealed record DeleteAppCommand(AppName AppName) : DeploymentCommandBase<DeleteAppCommand, AppInfo>(AppName);