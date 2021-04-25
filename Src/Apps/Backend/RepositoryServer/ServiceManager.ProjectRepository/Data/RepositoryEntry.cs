namespace ServiceManager.ProjectRepository.Data
{
    public sealed record RepositoryEntry(string RepoName, string FileName, string SourceUrl, string Id, string LastUpdate, bool IsUploaded, long RepoId)
    {
        public RepositoryEntry()
            : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false, -1)
        { }
    }
}