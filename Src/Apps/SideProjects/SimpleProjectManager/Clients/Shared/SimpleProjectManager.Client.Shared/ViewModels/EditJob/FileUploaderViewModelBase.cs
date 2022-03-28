using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Alias;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.Files;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;

namespace SimpleProjectManager.Client.Shared.ViewModels.EditJob;

public abstract class FileUploaderViewModelBase : ViewModelBase
{
    private readonly IMessageMapper _aggregator;
    private readonly GlobalState _globalState;
    private readonly SourceCache<FileUploadFile, string> _files = new(bf => bf.Name);
    private readonly BehaviorSubject<bool> _isUploading = new(false);
    private readonly ObservableAsPropertyHelper<bool> _shouldDisable;
    private readonly SerialDisposable _triggerSubscribe;

    private string? _projectId;

    private bool ShouldDisable => _shouldDisable.Value;
    
    public ReactiveCommand<Unit, Unit> Clear { get; }
    
    public ReactiveCommand<Unit, string> Upload { get; }

    public ReactiveCommand<FileChangeEvent, Unit> FilesChanged { get; }

    public Func<string, string> ValidateName { get; }

    public ReadOnlyObservableCollection<FileUploadFile> Files { get; }

    public string? ProjectId
    {
        get => _projectId;
        set => this.RaiseAndSetIfChanged(ref _projectId, value);
    }

    protected FileUploaderViewModelBase(IMessageMapper aggregator, GlobalState globalState)
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        var (triggerUpload, nameState) = GetModelInformation();

        _aggregator = aggregator;
        _globalState = globalState;
        _triggerSubscribe = 
            new SerialDisposable()
           .DisposeWith(Disposer);
        
        nameState.Subscribe(s => ProjectId = s).DisposeWith(Disposer);

        ValidateName = globalState.Jobs.ValidateProjectName;

        _files.DisposeWith(Disposer);

        _files.Connect()
           .AutoRefresh(f => f.UploadState, TimeSpan.FromMilliseconds(200))
           .Bind(out var list)
           .Subscribe()
           .DisposeWith(Disposer);

        _files.Connect()
           .Where(f => f.UploadState == UploadState.Pending && string.IsNullOrWhiteSpace(ProjectId))
           .Select(f => f.Name)
           .Select(globalState.Jobs.TryExtrectName)
           .Flatten()
           .Where(n => !string.IsNullOrWhiteSpace(n.Current) && string.IsNullOrWhiteSpace(ProjectId))
           .Do(n => ProjectId = n.Current)
           .Subscribe()
           .DisposeWith(Disposer);

        Files = list;

        var canExecute =
            _isUploading.CombineLatest(_files.CountChanged).Select(d => !d.First && d.Second > 0)
               .AndIsOnline(globalState.OnlineMonitor)
               .Publish().RefCount();

        _shouldDisable = canExecute.Select(canExecuteResult => !canExecuteResult).ToProperty(this, m => m.ShouldDisable);

        Upload = ReactiveCommand.CreateFromTask(
                UploadFiles,
                canExecute.CombineLatest(this.WhenAnyValue(m => m.ProjectId)).Select(t => t.First && !string.IsNullOrWhiteSpace(t.Second)))
           .DisposeWith(Disposer);
        Upload.IsExecuting.Subscribe(_isUploading).DisposeWith(Disposer);

        Clear = ReactiveCommand.Create(() => _files.Clear(), canExecute)
           .DisposeWith(Disposer);

        FilesChanged = ReactiveCommand.Create<FileChangeEvent, Unit>(
                args =>
                {
                    _files.Edit(
                        u => u.Load(
                            args.Files
                               .Where(_aggregator.IsSuccess<IFileReference>(FilesState.IsFileValid))
                               .Select(bf => new FileUploadFile(bf))));

                    return Unit.Default;
                })
           .DisposeWith(Disposer);

        triggerUpload
           .NotNull()
           .Subscribe(NewTrigger)
           .DisposeWith(Disposer);
    }

    protected abstract (IObservable<FileUploadTrigger> triggerUpload, IState<string> nameState) GetModelInformation();
    
    private void NewTrigger(FileUploadTrigger trigger)
        => _triggerSubscribe.Disposable = trigger.Set(UploadFiles);

    private async Task<string> UploadFiles()
    {
        if(Files.Count == 0) return string.Empty;

        try
        {
            var validation = _globalState.Jobs.ValidateProjectName(ProjectId);
            if (!string.IsNullOrWhiteSpace(validation))
            {
                _aggregator.PublishWarnig(validation);
                return validation;
            }

            if (string.IsNullOrWhiteSpace(ProjectId)) return "Keine Projekt Id";

            var context = new UploadTransactionContext(Files.ToImmutableList(), new ProjectName(ProjectId));

            var (trasnactionState, exception) = await _globalState.Files.CreateUpload().Execute(context);

            switch (trasnactionState)
            {
                case TrasnactionState.Successeded:
                    return string.Empty;
                case TrasnactionState.RollbackFailed when exception is not null:
                    try
                    {
                        var newEx = exception.Demystify();

                        _globalState.Dispatch(
                            new WriteCriticalError(
                                DateTime.Now,
                                $"{nameof(FileUploaderViewModelBase)} -- {nameof(UploadFiles)}",
                                newEx.Message,
                                newEx.StackTrace,
                                ImmutableList<ErrorProperty>.Empty));
                    }
                    catch (Exception e)
                    {
                        _aggregator.PublishError(e);
                    }
                    finally
                    {
                        _aggregator.PublishError(exception);
                    }
                    return exception.Message;
                case TrasnactionState.Rollback when exception is not null:
                    _aggregator.PublishError(exception);

                    return exception.Message;
                default:
                    _aggregator.PublishWarnig("Datei Upload Unbekannter zustand");

                    return "Datei Upload Unbekannter zustand";
            }
        }
        finally
        {
            _files.KeyValues.Select(p => p.Value).Foreach(f => f.UploadState = UploadState.Compled);
        }
    }
}