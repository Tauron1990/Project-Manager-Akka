using System;
using System.Threading.Tasks;

namespace SimpleProjectManager.Client.Shared.ViewModels.EditJob;

public sealed class FileUploadTrigger : IDisposable
{
    private Func<Task<string>>? _run;

    public IDisposable Set(Func<Task<string>> runner)
    {
        _run = runner;

        return this;
    }

    public async Task<string> Upload()
    {
        if (_run is null)
            throw new InvalidOperationException("No Runner Set");

        return await _run();
    }

    public void Dispose()
        => _run = null;
}