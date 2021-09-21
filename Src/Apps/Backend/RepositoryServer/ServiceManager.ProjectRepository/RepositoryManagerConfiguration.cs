using SharpRepository.Repository.Configuration;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.VirtualFiles;

namespace ServiceManager.ProjectRepository
{
    public record RepositoryManagerConfiguration(ISharpRepositoryConfiguration RepositoryConfiguration, IDirectory FileSystem, DataTransferManager DataTransferManager);
}