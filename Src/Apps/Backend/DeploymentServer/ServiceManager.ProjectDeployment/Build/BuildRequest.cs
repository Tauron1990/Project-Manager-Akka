using System.Threading.Tasks;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkaNode.Services.Reporting;
using Tauron.Application.Master.Commands.Deployment;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.Workshop;
using Tauron.Temp;

namespace ServiceManager.ProjectDeployment.Build
{
    public sealed record BuildRequest(Reporter Source, AppData AppData, RepositoryApi RepositoryApi, ITempFile TargetFile, TaskCompletionSource<(RepositoryCommit, ITempFile)> CompletionSource)
    {
        public static Task<(RepositoryCommit Commit, ITempFile File)> SendWork(
            IWorkDistributor<BuildRequest> distributor, Reporter source, AppData appData,
            RepositoryApi repositoryApi, ITempFile targetFile)
        {
            var request = new BuildRequest(source, appData, repositoryApi, targetFile, new TaskCompletionSource<(RepositoryCommit, ITempFile)>());
            distributor.PushWork(request);

            return request.CompletionSource.Task;
        }
    }
}