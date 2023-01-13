using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands;

public sealed record CreateAppCommand(AppName AppName, RepositoryName TargetRepo, ProjectName ProjectName) : DeploymentCommandBase<CreateAppCommand, AppInfo>(AppName);