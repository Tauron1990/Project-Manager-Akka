using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobFileService
{
    [ComputeMethod]
    Task<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token);

    Task<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token);
}