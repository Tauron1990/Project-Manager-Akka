using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using ServiceManager.Client.ViewModels.Apps;
using Tauron.Application.Master.Commands;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Build;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Client.Shared.Apps.NewApp
{
    public sealed record AppRegistrationInfo(RepositoryName Repository, ProjectName ProjectName, AppName AppName, ImmutableList<string> Projects, bool RequestRepository)
    {
        public AppRegistrationInfo()
            : this(RepositoryName.Empty, ProjectName.Empty, AppName.Empty, ImmutableList<string>.Empty, RequestRepository: true)
        { }

        public Task<LocalAppInfo> ToAppInfo()
            => Task.FromResult(
                new LocalAppInfo(
                    new AppInfo(AppName, SimpleVersion.NoVersion, DateTime.MinValue, DateTime.Now, Repository, ImmutableList<AppBinary>.Empty, Deleted: false, ProjectName)));
    }
}