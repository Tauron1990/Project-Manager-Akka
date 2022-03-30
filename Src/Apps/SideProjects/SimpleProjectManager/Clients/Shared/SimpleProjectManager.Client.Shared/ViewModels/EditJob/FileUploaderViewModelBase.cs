﻿using System;
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
    private SourceCache<FileUploadFile, string>? _files;
    private ObservableAsPropertyHelper<bool>? _shouldDisable;
    private SerialDisposable? _triggerSubscribe;

    private string? _projectId;

    private bool ShouldDisable => _shouldDisable?.Value ?? false;
    
    public ReactiveCommand<Unit, Unit>? Clear { get; private set; }
    
    public ReactiveCommand<Unit, string>? Upload { get; private set; }

    public ReactiveCommand<FileChangeEvent, Unit>? FilesChanged { get; private set; }

    public Func<string, string> ValidateName { get; }

    public ReadOnlyObservableCollection<FileUploadFile>? Files { get; private set; }

    public string? ProjectId
    {
        get => _projectId;
        set => this.RaiseAndSetIfChanged(ref _projectId, value);
    }

    protected FileUploaderViewModelBase(IMessageMapper aggregator, GlobalState globalState)
    {
        BehaviorSubject<bool>? isUploading;
        _aggregator = aggregator;
        _globalState = globalState;
        ValidateName = globalState.Jobs.ValidateProjectName;
        
        this.WhenActivated(Init);

        IEnumerable<IDisposable> Init()
        {
            var (triggerUpload, nameState) = GetModelInformation();

            yield return _triggerSubscribe = new SerialDisposable();
            yield return isUploading = new BehaviorSubject<bool>(false);
            yield return _files = new SourceCache<FileUploadFile, string>(bf => bf.Name);

            yield return nameState.Subscribe(s => ProjectId = s);

            yield return _files.Connect()
               .AutoRefresh(f => f.UploadState, TimeSpan.FromMilliseconds(200))
               .Bind(out var list)
               .Subscribe();

            yield return _files.Connect()
               .Where(f => f.UploadState == UploadState.Pending && string.IsNullOrWhiteSpace(ProjectId))
               .Select(f => f.Name)
               .Select(globalState.Jobs.TryExtrectName)
               .Flatten()
               .Where(n => !string.IsNullOrWhiteSpace(n.Current) && string.IsNullOrWhiteSpace(ProjectId))
               .Do(n => ProjectId = n.Current)
               .Subscribe();

            Files = list;

            var canExecute =
                isUploading.CombineLatest(_files.CountChanged).Select(d => !d.First && d.Second > 0)
                   .AndIsOnline(globalState.OnlineMonitor)
                   .Publish().RefCount();

            _shouldDisable = canExecute.Select(canExecuteResult => !canExecuteResult).ToProperty(this, m => m.ShouldDisable);

            yield return Upload = ReactiveCommand.CreateFromTask(
                UploadFiles,
                canExecute.CombineLatest(this.WhenAnyValue(m => m.ProjectId)).Select(t => t.First && !string.IsNullOrWhiteSpace(t.Second)));

            yield return Upload.IsExecuting.Subscribe(isUploading);

            yield return Clear = ReactiveCommand.Create(() => _files.Clear(), canExecute);

            yield return FilesChanged = ReactiveCommand.Create<FileChangeEvent, Unit>(
                args =>
                {
                    _files.Edit(
                        u => u.Load(
                            args.Files
                               .Where(_aggregator.IsSuccess<IFileReference>(FilesState.IsFileValid))
                               .Select(bf => new FileUploadFile(bf))));

                    return Unit.Default;
                });

            yield return triggerUpload
               .NotNull()
               .Subscribe(NewTrigger);
        }
    }

    protected abstract (IObservable<FileUploadTrigger> triggerUpload, IState<string> nameState) GetModelInformation();
    
    private void NewTrigger(FileUploadTrigger trigger)
        => NotNull(_triggerSubscribe, nameof(_triggerSubscribe)).Disposable = trigger.Set(UploadFiles);

    private async Task<string> UploadFiles()
    {
        if(Files is null || Files.Count == 0) return string.Empty;

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
            NotNull(_files, nameof(_files)).KeyValues.Select(p => p.Value).Foreach(f => f.UploadState = UploadState.Compled);
        }
    }
}