using SharpRepository.Repository.Configuration;
using YellowDrawer.Storage.Common;

namespace ServiceManager.ProjectRepository
{
    public record RepositoryManagerConfiguration(ISharpRepositoryConfiguration RepositoryConfiguration, IStorageProvider Provider);
}