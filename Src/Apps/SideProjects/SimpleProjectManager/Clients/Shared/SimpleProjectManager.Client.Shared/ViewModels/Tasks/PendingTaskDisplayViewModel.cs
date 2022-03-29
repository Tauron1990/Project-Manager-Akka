using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.Tasks;

public sealed class PendingTaskDisplayViewModel : ViewModelBase
{
    private readonly BehaviorSubject<bool> _canRun = new(true);

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    public PendingTaskDisplayViewModel(GlobalState globalState, IState<PendingTask?> taskState)
    {
        _canRun.DisposeWith(Disposer);

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
           .DisposeWith(Disposer);

        Cancel.Select(_ => false).Take(1).Subscribe(_canRun).DisposeWith(Disposer);
    }
}