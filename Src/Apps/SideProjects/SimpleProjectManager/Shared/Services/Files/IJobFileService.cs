using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface IJobFileService
{
    [ComputeMethod]
    ValueTask<ProjectFileInfo?> GetJobFileInfo(ProjectFileId id, CancellationToken token);

    ValueTask<string> RegisterFile(ProjectFileInfo projectFile, CancellationToken token);
}