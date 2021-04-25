using SharpRepository.Repository.Configuration;
using Tauron.Application.AkkaNode.Services.FileTransfer;
using Tauron.Application.Files.VirtualFiles;

namespace ServiceManager.ProjectRepository
{
    public record RepositoryManagerConfiguration(ISharpRepositoryConfiguration RepositoryConfiguration, IVirtualFileSystem FileSystem, DataTransferManager DataTransferManager);
}