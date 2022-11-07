using System.Diagnostics.CodeAnalysis;
using RestEase;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.ErrorApi), DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public interface ICriticalErrorServiceDef
{
    [Get(nameof(CountErrors))]
    Task<ErrorCount> CountErrors(CancellationToken token);
    
    [Get(nameof(GetErrors))]
    Task<CriticalErrorList> GetErrors(CancellationToken token);

    [Post(nameof(DisableError))]
    Task<SimpleResult> DisableError([Query]string id, CancellationToken token);
    
    [Post(nameof(WriteError))]
    Task WriteError([Body]CriticalError error, CancellationToken token);
}