using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services;

public interface IJobDatabaseService
{
    [ComputeMethod]
    Task<JobInfo[]> GetActiveJobs(CancellationToken token);

    Task<OperationResult> CreateJob(CreateProjectCommand command, CancellationToken token);
}