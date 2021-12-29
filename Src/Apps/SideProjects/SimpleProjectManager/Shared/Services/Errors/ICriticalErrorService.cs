using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface ICriticalErrorService
{
    [ComputeMethod(KeepAliveTime = 5), Swap(1)]
    Task<long> CountErrors(CancellationToken token);

    [ComputeMethod(KeepAliveTime = 5), Swap(1)]
    Task<CriticalError[]> GetErrors(CancellationToken token);

    Task<string> DisableError(string id, CancellationToken token);
    
    Task WriteError(CriticalError error, CancellationToken token);
}