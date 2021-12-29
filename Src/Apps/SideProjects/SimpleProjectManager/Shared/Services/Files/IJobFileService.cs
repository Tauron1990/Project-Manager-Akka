using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobFileService
{
    [ComputeMethod(KeepAliveTime = 5), Swap(1)]
    Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token);

    [ComputeMethod(KeepAliveTime = 5), Swap(1)]
    Task<DatabaseFile[]> GetAllFiles(CancellationToken token);

    Task<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token);

    Task<string> CommitFiles(FileList files, CancellationToken token);

    Task<string> DeleteFiles(FileList files, CancellationToken token);
}