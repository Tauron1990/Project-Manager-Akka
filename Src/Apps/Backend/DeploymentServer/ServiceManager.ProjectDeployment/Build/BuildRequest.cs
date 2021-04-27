using System.Threading.Tasks;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.Workshop;
using Tauron.Temp;

namespace ServiceManager.ProjectDeployment.Build
{
    public sealed record BuildRequest(Reporter Source, AppData AppData, RepositoryApi RepositoryApi, ITempFile TargetFile, TaskCompletionSource<(string, ITempFile)> CompletionSource)
    {
        public static Task<(string, ITempFile)> SendWork(IWorkDistributor<BuildRequest> distributor, Reporter source, AppData appData, RepositoryApi repositoryApi, ITempFile targetFile)
        {
            var request = new BuildRequest(source, appData, repositoryApi, targetFile, new TaskCompletionSource<(string, ITempFile)>());
            distributor.PushWork(request);
            return request.CompletionSource.Task;
        }
    }
}