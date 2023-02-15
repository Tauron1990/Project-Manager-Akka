using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;

namespace ServiceManager.Shared.Apps;

public sealed record DefaultAppConfig(AppName Name, RepositoryName Repository, ProjectName ProjectName)
{
    public DefaultAppConfig(string name, string repository, string projectName)
        : this(AppName.From(name), RepositoryName.From(repository), ProjectName.From(projectName))
    {
    }
}