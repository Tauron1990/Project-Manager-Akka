﻿using System.Threading.Tasks;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkaNode.Services;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Temp;

namespace ServiceManager.ProjectDeployment.Build
{
    public sealed class BuildRequest
    {
        private BuildRequest(Reporter source, AppData appData, RepositoryApi repositoryApi, ITempFile targetFile)
        {
            CompletionSource = new TaskCompletionSource<(string, ITempFile)>();
            Source = source;
            AppData = appData;
            RepositoryApi = repositoryApi;
            TargetFile = targetFile;
        }

        public Reporter Source { get; }

        public AppData AppData { get; }
        public RepositoryApi RepositoryApi { get; }

        public ITempFile TargetFile { get; }

        public TaskCompletionSource<(string, ITempFile)> CompletionSource { get; }

        public static Task<(string, ITempFile)> SendWork(IWorkDistributor<BuildRequest> distributor, Reporter source,
            AppData appData, RepositoryApi repositoryApi, ITempFile targetFile)
        {
            var request = new BuildRequest(source, appData, repositoryApi, targetFile);
            distributor.PushWork(request);
            return request.CompletionSource.Task;
        }
    }
}