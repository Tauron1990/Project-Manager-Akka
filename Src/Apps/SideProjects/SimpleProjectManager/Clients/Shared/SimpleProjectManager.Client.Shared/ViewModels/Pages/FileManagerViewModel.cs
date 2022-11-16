using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.Pages;

public sealed class FileManagerViewModel : ViewModelBase
{
    private readonly GlobalState _globalState;
    private readonly IMessageDispatcher _dispatcher;
    
    private ObservableAsPropertyHelper<Exception?>? _error;

    private ObservableAsPropertyHelper<DatabaseFile[]>? _files;

    private ObservableAsPropertyHelper<bool>? _loading;

    public FileManagerViewModel(GlobalState globalState, IMessageDispatcher dispatcher)
    {
        _globalState = globalState;
        _dispatcher = dispatcher;
        this.WhenActivated(Init);
    }

    private IEnumerable<IDisposable> Init()
    {
        var filesStream = _globalState.Files.AllFiles
           .Select(files => new FilesInfo(IsLoading: false, files))
           .StartWith(new FilesInfo(IsLoading: true, Array.Empty<DatabaseFile>()))
           .Publish().RefCount();

        yield return _files = filesStream.Select(i => i.Files).ToProperty(this, m => m.Files);
        yield return _loading = filesStream.Select(i => i.IsLoading).ToProperty(this, m => m.IsLoading);
        yield return _error = _files.ThrownExceptions.Merge(_loading.ThrownExceptions).ToProperty(this, m => m.Error);

        yield return DeleteFile = ReactiveCommand.CreateFromTask<DatabaseFile, Unit>(DeleteFileImpl, _globalState.IsOnline);
    }

    private async Task<Unit> DeleteFileImpl(DatabaseFile databaseFile)
    {
        if(await ConfirmDelete.Handle(databaseFile))
            await _dispatcher.IsSuccess(
                () => TimeoutToken.WithDefault(default, t => _globalState.Files.DeleteFile(databaseFile, t))).ConfigureAwait(false);

        return Unit.Default;
    }
    
    public bool IsLoading => _loading?.Value ?? false;
    public DatabaseFile[] Files => _files?.Value ?? Array.Empty<DatabaseFile>();
    public Exception? Error => _error?.Value;

    public ReactiveCommand<DatabaseFile, Unit>? DeleteFile { get; private set; }

    public Interaction<DatabaseFile, bool> ConfirmDelete { get; } = new();

    private record struct FilesInfo(bool IsLoading, DatabaseFile[] Files);
}