using SharpRepository.Repository.Configuration;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Master.Commands.Deployment.Repository;
using Tauron.Application.VirtualFiles;

namespace ServiceManager.ProjectDeployment
{
    public sealed record DeploymentConfiguration(ISharpRepositoryConfiguration Configuration, IDirectory FileSystem, DataTransferManager Manager, RepositoryApi RepositoryApi);
}