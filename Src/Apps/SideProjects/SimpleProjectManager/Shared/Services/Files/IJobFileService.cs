using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services;

public interface IJobFileService : IComputeService
{
    [ComputeMethod(MinCacheDuration = 5)]
    Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<DatabaseFile[]> GetAllFiles(CancellationToken token);

    Task<SimpleResult> RegisterFile(ProjectFileInfo projectFile, CancellationToken token);

    Task<SimpleResult> CommitFiles(FileList files, CancellationToken token);

    Task<SimpleResult> DeleteFiles(FileList files, CancellationToken token);
}