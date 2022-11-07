using System.Diagnostics.CodeAnalysis;
using RestEase;
using SimpleProjectManager.Shared.Services.Tasks;
using Tauron.Operations;

namespace SimpleProjectManager.Shared.ServerApi.RestApi;

[BasePath(ApiPaths.TaskApi), DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public interface ITaskManagerDef
{
    [Get(nameof(GetTasks))]
    Task<Tasks> GetTasks(CancellationToken token);

    [Post(nameof(DeleteTask))]
    Task<SimpleResult> DeleteTask([Body]string id, CancellationToken token);
}