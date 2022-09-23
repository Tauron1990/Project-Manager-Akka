using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
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
    protected record ModelConfig(
        IDisposableState<IEventCallback<JobEditorCommit>> CommitEvent, IDisposableState<IEventCallback> CancelEvent,
        IState<bool> CanCancel, IObservable<JobEditorData?> JobData);

    private BehaviorSubject<bool>? _isValid;

    public bool IsValid => _isValid?.Value ?? false;
    public Action<bool> IsValidChanged => NotNull(_isValid, nameof(_isValid)).OnNext;

    private ObservableAsPropertyHelper<JobEditorData?>? _data;

    public JobEditorData? Data => _data?.Value;

    public ReactiveCommand<Unit, Unit>? Cancel { get; private set; }
    
    public ReactiveCommand<Unit, Unit>? Commit { get; private set; }

    public FileUploadTrigger FileUploadTrigger { get; }

    public FileUploaderViewModelBase UploaderViewModel { get; }

    public Func<string?, IEnumerable<string>> ProjectNameValidator { get; }
    
    public Func<DateTime, IEnumerable<string>> DeadlineValidator { get; }

    protected JobEditorViewModelBase(IMessageMapper mapper, FileUploaderViewModelBase uploaderViewModel, GlobalState globalState)
    {
        var deadlineValidator = new ProjectDeadlineValidator();
        
        ProjectNameValidator = globalState.Jobs.ValidateProjectName;
        DeadlineValidator = time =>
                            {
                                var validation = deadlineValidator.Validate(new ProjectDeadline(time));
                                return validation.IsValid ? Array.Empty<string>() : validation.Errors.Select(err => err.ErrorMessage);

                            };
        
        FileUploadTrigger = new FileUploadTrigger();
        UploaderViewModel = uploaderViewModel;
        
        this.WhenActivated(Init);
        
        IEnumerable<IDisposable> Init()
        {
            var modelConfig = GetModelConfiguration();

            yield return _isValid = new BehaviorSubject<bool>(false);
            
            var commitEvent = modelConfig.CommitEvent;
            var cancelEvent = modelConfig.CancelEvent;
            var canCancel = modelConfig.CanCancel;

            yield return commitEvent;
            yield return cancelEvent;
            
            yield return _data = modelConfig.JobData
               .Select(i => i ?? new JobEditorData(null))
               .ObserveOn(RxApp.MainThreadScheduler)
               .ToProperty(this, m => m.Data);

            yield return Cancel = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    if (!cancelEvent.HasValue) return;

                    await cancelEvent.Value.InvokeAsync();
                },
                canCancel.ToObservable().StartWith(false));

            yield return Commit = ReactiveCommand.CreateFromTask(CreateCommit, _isValid.AndIsOnline(globalState.OnlineMonitor));
            
            async Task CreateCommit()
            {
                if(!commitEvent.HasValue) return;

                if (Data == null)
                {
                    mapper.PublishError("Keine Daten Verfügbar");
                    return;
                }

                await commitEvent.Value.InvokeAsync(globalState.Jobs.CreateNewJobData(Data, FileUploadTrigger.Upload));
            }
        }
    }

    protected abstract ModelConfig GetModelConfiguration();

}