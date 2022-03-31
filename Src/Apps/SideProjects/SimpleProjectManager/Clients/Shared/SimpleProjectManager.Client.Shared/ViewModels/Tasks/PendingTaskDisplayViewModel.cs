using System;
using System.Collections.Generic;
using System.Reactive;
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
    public ReactiveCommand<Unit, Unit>? Cancel { get; private set; }

    public PendingTaskDisplayViewModel(GlobalState globalState, IState<PendingTask?> taskState)
    {
        BehaviorSubject<bool>? canRun;
        this.WhenActivated(Init);
        
        IEnumerable<IDisposable> Init()
        {
            yield return canRun = new BehaviorSubject<bool>(false);
            yield return Cancel = ReactiveCommand.Create(
                () =>
                {
                    var task = taskState.ValueOrDefault;
                    if (task == null) return;

                    globalState.Dispatch(new DeleteTask(task));
                },
                canRun.CombineLatest(taskState.ToObservable().Select(pt => pt is not null))
                   .Select(i => i.First && i.Second)
                   .AndIsOnline(globalState.OnlineMonitor));
            
            yield return Cancel.Select(_ => false).Take(1).Subscribe(canRun);
        }
    }
}