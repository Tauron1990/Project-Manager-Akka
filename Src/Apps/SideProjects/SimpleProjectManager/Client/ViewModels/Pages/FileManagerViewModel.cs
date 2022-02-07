using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Shared;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileManagerViewModel : BlazorViewModel
{
    private record struct FilesInfo(bool IsLoading, DatabaseFile[] Files);
    
    private readonly ObservableAsPropertyHelper<bool> _loading;
    public bool IsLoading => _loading.Value;

    private readonly ObservableAsPropertyHelper<DatabaseFile[]> _files;
    public DatabaseFile[] Files => _files.Value;

    private readonly ObservableAsPropertyHelper<Exception?> _error;
    public Exception? Error => _error.Value;

    public Action<DatabaseFile> DeleteFile { get; }

    public Interaction<DatabaseFile, bool> ConfirmDelete { get; } = new();
    
    public FileManagerViewModel(IStateFactory stateFactory, GlobalState globalState, IEventAggregator aggregator)
        : base(stateFactory)
    {
        var filesStream = globalState.Files.AllFiles
           .Select(files => new FilesInfo(false, files))
           .StartWith(new FilesInfo(true, Array.Empty<DatabaseFile>()))
           .Publish().RefCount();
        
        _files = filesStream.Select(i => i.Files).ToProperty(this, m => m.Files);
        _loading = filesStream.Select(i => i.IsLoading).ToProperty(this, m => m.IsLoading);
        _error = _files.ThrownExceptions.Merge(_loading.ThrownExceptions).ToProperty(this, m => m.Error);
        
        var command = ReactiveCommand.CreateFromTask<DatabaseFile, Unit>(DeleteFileImpl, globalState.IsOnline).DisposeWith(this);
        
        DeleteFile = command.ToAction();
        
        async Task<Unit> DeleteFileImpl(DatabaseFile databaseFile)
        {
            if (await ConfirmDelete.Handle(databaseFile))
            {
                await aggregator.IsSuccess(
                    () => TimeoutToken.WithDefault(default, t => globalState.Files.DeleteFile(databaseFile, t)));
            }
        
            return Unit.Default;
        }
    }
}