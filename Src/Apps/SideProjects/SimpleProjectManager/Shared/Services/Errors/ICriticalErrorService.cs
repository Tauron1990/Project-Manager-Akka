using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface ICriticalErrorService
{
    [ComputeMethod]
    ValueTask<long> CountErrors(CancellationToken token);

    [ComputeMethod]
    ValueTask<CriticalError[]> GetErrors(CancellationToken token);

    ValueTask<string> DisableError(string id, CancellationToken token);
    
    ValueTask WriteError(CriticalError error, CancellationToken token);
}