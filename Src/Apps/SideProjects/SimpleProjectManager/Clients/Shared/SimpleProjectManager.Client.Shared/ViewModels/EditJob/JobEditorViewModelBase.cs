using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using SimpleProjectManager.Client.Shared.Services;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.EditJob;

public abstract class JobEditorViewModelBase : ViewModelBase
{
    protected record ModelConfig(
        IState<IEventCallback<JobEditorCommit>> CommitEvent, IState<IEventCallback> CancelEvent,
        IState<bool> CanCancel, IObservable<JobEditorData?> JobData);

    private readonly BehaviorSubject<bool> _isValid;

    public bool IsValid => _isValid.Value;
    public Action<bool> IsValidChanged => _isValid.OnNext;

    private readonly ObservableAsPropertyHelper<JobEditorData?> _data;

    public JobEditorData? Data => _data.Value;

    public ReactiveCommand<Unit, Unit> Cancel { get; }
    
    public ReactiveCommand<Unit, Unit> Commit { get; }

    public FileUploadTrigger FileUploadTrigger { get; }

    public FileUploaderViewModelBase UploaderViewModel { get; }

    protected JobEditorViewModelBase(IMessageMapper mapper, FileUploaderViewModelBase uploaderViewModel, GlobalState globalState,
        ModelConfig modelConfig)
    {
        UploaderViewModel = uploaderViewModel;

        _isValid = new BehaviorSubject<bool>(false).DisposeWith(this);
        FileUploadTrigger = new FileUploadTrigger();

        var commitEvent = modelConfig.CommitEvent;
        var cancelEvent = modelConfig.CancelEvent;
        var canCancel = modelConfig.CanCancel;

        _data = modelConfig.JobData
           .Select(i => i ?? new JobEditorData(null))
           .ToProperty(this, m => m.Data)
           .DisposeWith(this);

        Cancel = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if (!cancelEvent.HasValue) return;

                await cancelEvent.Value.InvokeAsync();
            }, canCancel.ToObservable().StartWith(false))
           .DisposeWith(this);

        Commit = ReactiveCommand.CreateFromTask(CreateCommit, _isValid.AndIsOnline(globalState.OnlineMonitor));

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