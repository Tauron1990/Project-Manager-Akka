using Stl.Fusion;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.Services;

public interface ICriticalErrorService
{
    [ComputeMethod(MinCacheDuration = 5)]
    Task<ErrorCount> CountErrors(CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<CriticalErrorList> GetErrors(CancellationToken token);

    Task<SimpleResult> DisableError(ErrorId id, CancellationToken token);
    
    Task WriteError(CriticalError error, CancellationToken token);
}