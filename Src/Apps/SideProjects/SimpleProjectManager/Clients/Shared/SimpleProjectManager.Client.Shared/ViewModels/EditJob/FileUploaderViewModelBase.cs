using System;
using System.Collections.Generic;
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
using SimpleProjectManager.Client.Shared.Data.States.JobState;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Operations;

namespace SimpleProjectManager.Client.Shared.ViewModels.EditJob;

public abstract class FileUploaderViewModelBase : ViewModelBase
{
    private readonly GlobalState _globalState;

    private SourceCache<FileUploadFile, FileName>? _files;
    private BehaviorSubject<bool>? _isUploading;

    private string? _projectId;
    private ObservableAsPropertyHelper<bool>? _shouldDisable;
    private SerialDisposable? _triggerSubscribe;

    protected FileUploaderViewModelBase(IMessageDispatcher aggregator, GlobalState globalState)
    {
        MessageDispatcher = aggregator;
        _globalState = globalState;
        ValidateName = globalState.Jobs.ValidateProjectName;

        this.WhenActivated(Init);
    }

    protected IMessageDispatcher MessageDispatcher { get; }

    private bool ShouldDisable => _shouldDisable?.Value ?? false;

    public ReactiveCommand<Unit, Unit>? Clear { get; private set; }

    public ReactiveCommand<Unit, SimpleResult>? Upload { get; private set; }

    public ReactiveCommand<FileChangeEvent, Unit>? FilesChanged { get; private set; }

    public Func<string, IEnumerable<string>> ValidateName { get; }

    public ReadOnlyObservableCollection<FileUploadFile>? Files { get; private set; }

    public string? ProjectId
    {
        get => _projectId;
        set => this.RaiseAndSetIfChanged(ref _projectId, value);
    }

    #pragma warning disable MA0051
    private IEnumerable<IDisposable> Init()
        #pragma warning restore MA0051
    {
        var (triggerUpload, nameState) = GetModelInformation();

        yield return _triggerSubscribe = new SerialDisposable();
        yield return _isUploading = new BehaviorSubject<bool>(false);
        yield return _files = new SourceCache<FileUploadFile, FileName>(bf => bf.Name);

        yield return nameState.Subscribe(s => ProjectId = s);

        yield return _files.Connect()
           .AutoRefresh(f => f.UploadState, TimeSpan.FromMilliseconds(200))
           .ObserveOn(RxApp.MainThreadScheduler)
           .Bind(out var list)
           .Subscribe();

        yield return _files.Connect()
           .Where(f => f.UploadState == UploadState.Pending && string.IsNullOrWhiteSpace(ProjectId))
           .Select(f => f.Name)
           .Select(JobsState.TryExtrectName)
           .Flatten()
           .Where(n => !string.IsNullOrWhiteSpace(n.Current) && string.IsNullOrWhiteSpace(ProjectId))
           .ObserveOn(RxApp.MainThreadScheduler)
           .Do(n => ProjectId = n.Current)
           .Subscribe();

        Files = list;

        var canExecute =
            _isUploading.CombineLatest(_files.CountChanged).Select(d => !d.First && d.Second > 0)
               .AndIsOnline(_globalState.OnlineMonitor)
               .Publish().RefCount();

        _shouldDisable = canExecute.Select(canExecuteResult => !canExecuteResult).ToProperty(this, m => m.ShouldDisable);

        yield return Upload = ReactiveCommand.CreateFromTask(
            UploadFiles,
            canExecute.CombineLatest(this.WhenAnyValue(m => m.ProjectId)).Select(t => t.First && !string.IsNullOrWhiteSpace(t.Second)));

        yield return Upload.IsExecuting.Subscribe(_isUploading);

        yield return Clear = ReactiveCommand.Create(() => _files.Clear(), _files.CountChanged.Select(c => c != 0));
        yield return Upload
           .ObserveOn(RxApp.MainThreadScheduler)
           .Do(
                s =>
                {
                    if(s.IsSuccess())
                        MessageDispatcher.PublishMessage("Upload Erfolgreich");
                    else
                        MessageDispatcher.PublishError($"Fehler beim Upload: {s.GetErrorString()}");
                })
           .Select(_ => Unit.Default)
           .InvokeCommand(Clear);

        yield return FilesChanged = ReactiveCommand.Create<FileChangeEvent, Unit>(
            args =>
            {
                _files.Edit(
                    u => u.Load(
                        args.Files
                           .Where(MessageDispatcher.IsSuccess<IFileReference>(FilesState.IsFileValid))
                           .Select(bf => new FileUploadFile(bf))));

                return Unit.Default;
            });

        yield return triggerUpload
           .NotNull()
           .Subscribe(NewTrigger);
    }

    protected abstract (IObservable<FileUploadTrigger> triggerUpload, IState<string> nameState) GetModelInformation();

    private void NewTrigger(FileUploadTrigger trigger)
        => NotNull(_triggerSubscribe, nameof(_triggerSubscribe)).Disposable = trigger.Set(UploadFiles);

    #pragma warning disable MA0051
    private async Task<SimpleResult> UploadFiles()
        #pragma warning restore MA0051
    {
        if(Files is null || Files.Count == 0) return SimpleResult.Success();

        try
        {
            string[] validationResult = _globalState.Jobs.ValidateProjectName(ProjectId).AsOrToArray();
            if(validationResult.Length != 0)
            {
                string validation = string.Join(", ", validationResult);
                MessageDispatcher.PublishWarnig(validation);

                return SimpleResult.Failure(validation);
            }

            if(string.IsNullOrWhiteSpace(ProjectId)) return SimpleResult.Failure("Keine Projekt Id");

            var context = new UploadTransactionContext(Files.ToImmutableList(), new ProjectName(ProjectId));

            (TrasnactionState trasnactionState, Exception? exception) = await _globalState.Files.CreateUpload().Execute(context).ConfigureAwait(false);

            switch (trasnactionState)
            {
                case TrasnactionState.Successeded:
                    return SimpleResult.Success();
                case TrasnactionState.RollbackFailed when exception is not null:
                    try
                    {
                        Exception newEx = exception.Demystify();

                        _globalState.Dispatch(
                            new WriteCriticalError(
                                DateTime.Now,
                                PropertyValue.From($"{nameof(FileUploaderViewModelBase)} -- {nameof(UploadFiles)}"),
                                SimpleMessage.From(newEx.Message),
                                StackTraceData.FromException(newEx),
                                ImmutableList<ErrorProperty>.Empty));
                    }
                    catch (Exception e)
                    {
                        MessageDispatcher.PublishError(e);
                    }
                    finally
                    {
                        MessageDispatcher.PublishError(exception);
                    }

                    return SimpleResult.Failure(exception);
                case TrasnactionState.Rollback when exception is not null:
                    MessageDispatcher.PublishError(exception);

                    return SimpleResult.Failure(exception);
                default:
                    MessageDispatcher.PublishWarnig("Datei Upload Unbekannter zustand");

                    return SimpleResult.Failure("Datei Upload Unbekannter zustand");
            }
        }
        finally
        {
            NotNull(_files, nameof(_files)).KeyValues.Select(p => p.Value).Foreach(f => f.UploadState = UploadState.Compled);
        }
    }
}