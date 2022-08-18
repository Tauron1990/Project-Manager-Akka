﻿using Stl.Fusion;

namespace SimpleProjectManager.Shared.Services;

public interface ICriticalErrorService
{
    [ComputeMethod(MinCacheDuration = 5)]
    Task<long> CountErrors(CancellationToken token);

    [ComputeMethod(MinCacheDuration = 5)]
    Task<CriticalError[]> GetErrors(CancellationToken token);

    Task<string> DisableError(string id, CancellationToken token);
    
    Task WriteError(CriticalError error, CancellationToken token);
}