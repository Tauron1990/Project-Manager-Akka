using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobFileService
{
    [ComputeMethod]
    ValueTask<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token);

    [ComputeMethod]
    ValueTask<DatabaseFile[]> GetAllFiles(CancellationToken token);

    ValueTask<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token);

    ValueTask<string> CommitFiles(FileList files, CancellationToken token);

    ValueTask<string> DeleteFiles(FileList files, CancellationToken token);
}