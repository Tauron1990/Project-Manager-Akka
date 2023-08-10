using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services;

public interface IJobFileService : IComputeService
{
    [ComputeMethod(MinCacheDuration = 5)]
    Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<DatabaseFile[]> GetAllFiles(CancellationToken token);

    Task<SimpleResultContainer> RegisterFile(ProjectFileInfo projectFile, CancellationToken token);

    Task<SimpleResultContainer> CommitFiles(FileList files, CancellationToken token);

    Task<SimpleResultContainer> DeleteFiles(FileList files, CancellationToken token);
}