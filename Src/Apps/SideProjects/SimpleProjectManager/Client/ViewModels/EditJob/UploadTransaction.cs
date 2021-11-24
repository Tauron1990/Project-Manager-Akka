using System.Collections.Immutable;
using SimpleProjectManager.Client.Core;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion.Client.Internal;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed record UploadTransactionContext(ImmutableList<FileUploadFile> Files, string JobName, IJobFileService JobFileService, ClientAccessor<IJobFileServiceDef> ClientAccessor)

public sealed class UploadTransaction : SimpleTransaction<UploadTransactionContext>
{
    public UploadTransaction()
    {
        Register(UploadFiles);
    }

    private async ValueTask<Rollback<UploadTransactionContext>> UploadFiles(UploadTransactionContext context)
    {
        var requestContent = new MultipartFormDataContent();
        
        var result = await TimeoutToken.WithDefault()
    }
}