using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobFileService
{
    [ComputeMethod(MinCacheDuration = 5)]
    Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<DatabaseFile[]> GetAllFiles(CancellationToken token);

    Task<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token);

    Task<string> CommitFiles(FileList files, CancellationToken token);

    Task<string> DeleteFiles(FileList files, CancellationToken token);
}