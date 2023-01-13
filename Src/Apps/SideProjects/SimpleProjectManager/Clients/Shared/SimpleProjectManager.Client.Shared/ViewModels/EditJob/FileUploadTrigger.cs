using System;
using System.Threading.Tasks;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.ViewModels.EditJob;

public sealed class FileUploadTrigger : IDisposable
{
    private Func<Task<SimpleResult>>? _run;

    public void Dispose()
        => _run = null;

    public IDisposable Set(Func<Task<SimpleResult>> runner)
    {
        _run = runner;

        return this;
    }

    public async Task<SimpleResult> Upload()
    {
        if(_run is null)
            throw new InvalidOperationException("No Runner Set");

        return await _run().ConfigureAwait(false);
    }
}