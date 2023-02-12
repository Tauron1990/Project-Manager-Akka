using Tauron.Application.Master.Commands.Deployment;

namespace ServiceManager.ProjectRepository.Data
{
    public sealed record RepositoryEntry(RepositoryName RepoName, string FileName, string SourceUrl, RepositoryName Id, string LastUpdate, bool IsUploaded, long RepoId)
    {
        public RepositoryEntry()
            : this(RepositoryName.Empty, string.Empty, string.Empty, RepositoryName.Empty, string.Empty, IsUploaded: false, RepoId: -1) { }
    }
}