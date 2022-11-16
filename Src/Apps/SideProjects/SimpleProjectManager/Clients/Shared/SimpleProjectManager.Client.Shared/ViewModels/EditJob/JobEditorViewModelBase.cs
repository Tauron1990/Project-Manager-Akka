using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentValidation.Results;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Validators;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.EditJob;

public abstract class JobEditorViewModelBase : ViewModelBase
{
    private readonly GlobalState _globalState;
    private ObservableAsPropertyHelper<JobEditorData?>? _data;

    private BehaviorSubject<bool>? _isValid;

    protected JobEditorViewModelBase(IMessageDispatcher dispatcher, FileUploaderViewModelBase uploaderViewModel, GlobalState globalState)
    {
        _globalState = globalState;
        var deadlineValidator = new ProjectDeadlineValidator();

        ProjectNameValidator = globalState.Jobs.ValidateProjectName;
        DeadlineValidator = time =>
                            {
                                ValidationResult? validation = deadlineValidator.Validate(new ProjectDeadline(time));

                                return validation.IsValid ? Array.Empty<string>() : validation.Errors.Select(err => err.ErrorMessage);

                            };

        FileUploadTrigger = new FileUploadTrigger();
        MessageDispatcher = dispatcher;
        UploaderViewModel = uploaderViewModel;

        this.WhenActivated(Init);
    }

    private IEnumerable<IDisposable> Init()
    {
        ModelConfig modelConfig = GetModelConfiguration();

        yield return _isValid = new BehaviorSubject<bool>(value: false);

        var commitEvent = modelConfig.CommitEvent;
        var cancelEvent = modelConfig.CancelEvent;
        var canCancel = modelConfig.CanCancel;

        yield return commitEvent;
        yield return cancelEvent;

        yield return _data = modelConfig.JobData
           .Select(i => i ?? new JobEditorData(originalData: null))
           .ObserveOn(RxApp.MainThreadScheduler)
           .ToProperty(this, m => m.Data);

        yield return Cancel = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if(!cancelEvent.HasValue) return;

                await cancelEvent.Value.InvokeAsync().ConfigureAwait(false);
            },
            canCancel.ToObservable(MessageDispatcher.IgnoreErrors()).StartWith(false));

        yield return Commit = ReactiveCommand.CreateFromTask(CreateCommit, _isValid.AndIsOnline(_globalState.OnlineMonitor));

        async Task CreateCommit()
        {
            if(!commitEvent.HasValue) return;

            if(Data is null)
            {
                MessageDispatcher.PublishError("Keine Daten Verfügbar");

                return;
            }

            await commitEvent.Value.InvokeAsync(_globalState.Jobs.CreateNewJobData(Data, FileUploadTrigger.Upload)).ConfigureAwait(false);
        }
    }
    
    public bool IsValid => _isValid?.Value ?? false;
    public Action<bool> IsValidChanged => NotNull(_isValid, nameof(_isValid)).OnNext;

    public JobEditorData? Data => _data?.Value;

    public ReactiveCommand<Unit, Unit>? Cancel { get; private set; }

    public ReactiveCommand<Unit, Unit>? Commit { get; private set; }

    public FileUploadTrigger FileUploadTrigger { get; }

    protected IMessageDispatcher MessageDispatcher { get; }
    public FileUploaderViewModelBase UploaderViewModel { get; }

    public Func<string?, IEnumerable<string>> ProjectNameValidator { get; }

    public Func<DateTime, IEnumerable<string>> DeadlineValidator { get; }

    protected abstract ModelConfig GetModelConfiguration();

    protected record ModelConfig(
        IDisposableState<IEventCallback<JobEditorCommit>> CommitEvent, IDisposableState<IEventCallback> CancelEvent,
        IState<bool> CanCancel, IObservable<JobEditorData?> JobData);
}