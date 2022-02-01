using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using DynamicData.Alias;
using Microsoft.AspNetCore.Components.Forms;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Data.States;
using SimpleProjectManager.Client.Shared.EditJob;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class FileUploaderViewModel : BlazorViewModel
{
    private readonly IEventAggregator _aggregator;
    private readonly GlobalState _globalState;
    private readonly SourceCache<FileUploadFile, string> _files = new(bf => bf.Name);
    private readonly BehaviorSubject<bool> _isUploading = new(false);
    private readonly ObservableAsPropertyHelper<bool> _shouldDisable;
    private readonly SerialDisposable _triggerSubscribe;

    private string? _projectId;

    private bool ShouldDisable => _shouldDisable.Value;
    
    public ReactiveCommand<Unit, Unit> Clear { get; }
    
    public ReactiveCommand<Unit, string> Upload { get; }

    public Action<InputFileChangeEventArgs> FilesChanged { get; }

    public Func<string, string> ValidateName { get; }

    public ReadOnlyObservableCollection<FileUploadFile> Files { get; }

    public string? ProjectId
    {
        get => _projectId;
        set => this.RaiseAndSetIfChanged(ref _projectId, value);
    }

    public FileUploaderViewModel(IStateFactory stateFactory, IEventAggregator aggregator, GlobalState globalState)
        : base(stateFactory)
    {
        Console.WriteLine("Inititlize View Model");

        _aggregator = aggregator;
        _globalState = globalState;
        _triggerSubscribe = 
            new SerialDisposable()
           .DisposeWith(this);

        var nameState = GetParameter<string>(nameof(FileUploader.ProjectName)).ToObservable().StartWith(string.Empty);

        nameState.Subscribe(s => ProjectId = s).DisposeWith(this);

        ValidateName = globalState.Jobs.ValidateProjectName;

        _files.DisposeWith(this);

        _files.Connect()
           .AutoRefresh(f => f.UploadState, TimeSpan.FromMilliseconds(200))
           .Bind(out var list)
           .Subscribe()
           .DisposeWith(this);

        _files.Connect()
           .Where(f => f.UploadState == UploadState.Pending && string.IsNullOrWhiteSpace(ProjectId))
           .Select(f => f.Name)
           .Select(globalState.Jobs.TryExtrectName)
           .Flatten()
           .Where(n => !string.IsNullOrWhiteSpace(n.Current) && string.IsNullOrWhiteSpace(ProjectId))
           .Do(n => ProjectId = n.Current)
           .Subscribe()
           .DisposeWith(this);

        Files = list;

        var canExecute =
            _isUploading.CombineLatest(_files.CountChanged).Select(d => !d.First && d.Second > 0)
               .AndIsOnline(globalState.OnlineMonitor)
               .Publish().RefCount();

        _shouldDisable = canExecute.Select(canExecuteResult => !canExecuteResult).ToProperty(this, m => m.ShouldDisable);

        Upload = ReactiveCommand.CreateFromTask(
                UploadFiles,
                canExecute.CombineLatest(this.WhenAnyValue(m => m.ProjectId)).Select(t => t.First && !string.IsNullOrWhiteSpace(t.Second)))
           .DisposeWith(this);
        Upload.IsExecuting.Subscribe(_isUploading).DisposeWith(this);

        Clear = ReactiveCommand.Create(() => _files.Clear(), canExecute)
           .DisposeWith(this);

        var filesChanged = ReactiveCommand.Create<InputFileChangeEventArgs, Unit>(
                args =>
                {
                    _files.Edit(
                        u => u.Load(
                            args.GetMultipleFiles()
                               .Where(_aggregator.IsSuccess<IBrowserFile>(FilesState.IsFileValid))
                               .Select(bf => new FileUploadFile(bf))));

                    return Unit.Default;
                })
           .DisposeWith(this);
        FilesChanged = filesChanged.ToAction();

        GetParameter<FileUploadTrigger>(nameof(FileUploader.UploadTrigger))
           .ToObservable()
           .NotNull()
           .Subscribe(NewTrigger)
           .DisposeWith(this);
    }

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
                                $"{nameof(FileUploaderViewModel)} -- {nameof(UploadFiles)}",
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