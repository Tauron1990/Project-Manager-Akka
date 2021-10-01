using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using ServiceManager.Client.ViewModels.Apps;
using Tauron.Application.Master.Commands.Deployment.Build.Data;

namespace ServiceManager.Client.Shared.Apps.NewApp
{
    public sealed record AppRegistrationInfo(string Repository, string ProjectName, string AppName, ImmutableList<string> Projects, bool RequestRepository)
    {
        public AppRegistrationInfo()
            : this(string.Empty, string.Empty, string.Empty, ImmutableList<string>.Empty, RequestRepository: true)
        { }

        public Task<LocalAppInfo> ToAppInfo()
            => Task.FromResult(
                new LocalAppInfo(
                    new AppInfo(AppName, -1, DateTime.MinValue, DateTime.Now, Repository, ImmutableList<AppBinary>.Empty, Deleted: false, ProjectName)));
    }
}