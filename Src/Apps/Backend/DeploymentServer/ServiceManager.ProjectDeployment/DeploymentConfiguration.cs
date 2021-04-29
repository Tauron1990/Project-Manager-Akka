using SharpRepository.Repository.Configuration;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment
{
    public sealed record DeploymentConfiguration(ISharpRepositoryConfiguration Configuration, IVirtualFileSystem FileSystem, DataTransferManager Manager, RepositoryApi RepositoryApi);
}