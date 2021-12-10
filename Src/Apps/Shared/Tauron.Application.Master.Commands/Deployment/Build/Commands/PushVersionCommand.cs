using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace Tauron.Application.Master.Commands.Deployment.Build.Commands;

public sealed record PushVersionCommand
    (string AppName) : DeploymentCommandBase<PushVersionCommand, AppBinary>(AppName);