namespace Tauron.Application.Master.Commands.Deployment.Build;

public interface IDeploymentCommand
{
    AppName AppName { get; }
}