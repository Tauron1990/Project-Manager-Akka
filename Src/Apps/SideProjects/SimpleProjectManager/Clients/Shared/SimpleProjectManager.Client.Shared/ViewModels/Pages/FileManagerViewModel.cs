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
    private record struct FilesInfo(bool IsLoading, DatabaseFile[] Files);
    
    private ObservableAsPropertyHelper<bool>? _loading;
    public bool IsLoading => _loading?.Value ?? false;

    private ObservableAsPropertyHelper<DatabaseFile[]>? _files;
    public DatabaseFile[] Files => _files?.Value ?? Array.Empty<DatabaseFile>();

    private ObservableAsPropertyHelper<Exception?>? _error;
    public Exception? Error => _error?.Value;

    public ReactiveCommand<DatabaseFile, Unit>? DeleteFile { get; private set; }

    public Interaction<DatabaseFile, bool> ConfirmDelete { get; } = new();
    
    public FileManagerViewModel(GlobalState globalState, IMessageMapper mapper)
    {
        this.WhenActivated(Init);
        
        IEnumerable<IDisposable> Init()
        {
            var filesStream = globalState.Files.AllFiles
               .Select(files => new FilesInfo(false, files))
               .StartWith(new FilesInfo(true, Array.Empty<DatabaseFile>()))
               .Publish().RefCount();
            
            yield return _files = filesStream.Select(i => i.Files).ToProperty(this, m => m.Files);
            yield return _loading = filesStream.Select(i => i.IsLoading).ToProperty(this, m => m.IsLoading);
            yield return _error = _files.ThrownExceptions.Merge(_loading.ThrownExceptions).ToProperty(this, m => m.Error);
            
            yield return DeleteFile = ReactiveCommand.CreateFromTask<DatabaseFile, Unit>(DeleteFileImpl, globalState.IsOnline);
        }

        async Task<Unit> DeleteFileImpl(DatabaseFile databaseFile)
        {
            if (await ConfirmDelete.Handle(databaseFile))
            {
                await mapper.IsSuccess(
                    () => TimeoutToken.WithDefault(default, t => globalState.Files.DeleteFile(databaseFile, t)));
            }
        
            return Unit.Default;
        }
    }
}