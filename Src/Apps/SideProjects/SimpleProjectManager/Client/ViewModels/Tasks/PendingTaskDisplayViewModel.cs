using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Data.States;
using SimpleProjectManager.Client.Shared.Tasks;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class PendingTaskDisplayViewModel : BlazorViewModel
{
    private readonly BehaviorSubject<bool> _canRun = new(true);

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    public PendingTaskDisplayViewModel(IStateFactory stateFactory, GlobalState globalState)
        : base(stateFactory)
    {
        var taskState = GetParameter<PendingTask?>(nameof(PendingTaskDisplay.PendingTask));
        _canRun.DisposeWith(this);

        Cancel = ReactiveCommand.Create(
                () =>
                {
                    var task = taskState.ValueOrDefault;
                    if (task == null) return;

                    globalState.Dispatch(new DeleteTask(task));
                },
                _canRun.CombineLatest(taskState.ToObservable().Select(pt => pt is not null))
                   .Select(i => i.First && i.Second)
                   .AndIsOnline(globalState.OnlineMonitor))
           .DisposeWith(this);

        Cancel.Select(_ => false).Take(1).Subscribe(_canRun).DisposeWith(this);
    }
}