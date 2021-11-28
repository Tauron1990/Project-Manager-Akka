using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileManagerViewModel : StatefulViewModel<DatabaseFile[]>
{
    private readonly IJobFileService _fileService;
    private readonly IEventAggregator _aggregator;

    private readonly ObservableAsPropertyHelper<bool> _isLoading;
    public bool IsLoading => _isLoading.Value;

    private readonly ObservableAsPropertyHelper<DatabaseFile[]> _files;
    public DatabaseFile[] Files => _files.Value;

    public Action<DatabaseFile> DeleteFile { get; }

    public Interaction<DatabaseFile, bool> ConfirmDelete { get; } = new();
    
    public FileManagerViewModel(IStateFactory stateFactory, IJobFileService fileService, IEventAggregator aggregator)
        : base(stateFactory)
    {
        _isLoading = Observable.FromEventPattern<Action<IState<DatabaseFile[]>, StateEventKind>, StateChangeEventArgs>(
            eh => (state, kind) => eh(state, new StateChangeEventArgs(kind)),
            h => State.AddEventHandler(StateEventKind.All, h),
            h => State.RemoveEventHandler(StateEventKind.All, h))
           .Select(ep => ep.EventArgs.EventKind != StateEventKind.Updated)
           .ToProperty(this, m => m.IsLoading, initialValue:true)
           .DisposeWith(this);

        _files = NextElement
           .ToProperty(this, m => m.Files, Array.Empty<DatabaseFile>())
           .DisposeWith(this);

        var command = ReactiveCommand.CreateFromTask<DatabaseFile, Unit>(DeleteFileImpl).DisposeWith(this);
        DeleteFile = command.ToAction();
        
        _fileService = fileService;
        _aggregator = aggregator;
    }

    private async Task<Unit> DeleteFileImpl(DatabaseFile arg)
    {
        if (await ConfirmDelete.Handle(arg))
        {
            await _aggregator.IsSuccess(
                () => TimeoutToken.WithDefault(default,
                    t => _fileService.DeleteFiles(new FileList(ImmutableList<ProjectFileId>.Empty.Add(arg.Id)), t)));
        }
        
        return Unit.Default;
    }

    protected override async Task<DatabaseFile[]> ComputeState(CancellationToken cancellationToken)
        => await _fileService.GetAllFiles(cancellationToken);
    
    private sealed class StateChangeEventArgs : EventArgs
    {
        public StateEventKind EventKind { get; }

        public StateChangeEventArgs(StateEventKind eventKind)
            => EventKind = eventKind;
    }
}