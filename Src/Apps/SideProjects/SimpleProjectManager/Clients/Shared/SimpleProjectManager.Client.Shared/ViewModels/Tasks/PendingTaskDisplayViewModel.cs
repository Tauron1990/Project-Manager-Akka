using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.Tasks;

public sealed class PendingTaskDisplayViewModel : ViewModelBase
{
    public IMutableState<PendingTask?> PendingTask { get; set; }
    
    public ReactiveCommand<Unit, Unit>? Cancel { get; private set; }

    public PendingTaskDisplayViewModel(GlobalState globalState, IStateFactory stateFactory, IMessageDispatcher messageDispatcher)
    {
        PendingTask = stateFactory.NewMutable<PendingTask?>();
        
        BehaviorSubject<bool>? canRun;
        this.WhenActivated(Init);
        
        IEnumerable<IDisposable> Init()
        {
            yield return canRun = new BehaviorSubject<bool>(true);
            yield return Cancel = ReactiveCommand.Create(
                () =>
                {
                    var task = PendingTask.ValueOrDefault;
                    if (task == null) return;

                    globalState.Dispatch(new DeleteTask(task));
                },
                //PendingTask.ToObservable().Select(pt => pt is not null)
                 canRun.CombineLatest(PendingTask.ToObservable(messageDispatcher.IgnoreErrors()).Select(pt => pt is not null))
                    .Select(i => i.First && i.Second)
                    .AndIsOnline(globalState.OnlineMonitor)
                );
            
            yield return Cancel.Select(_ => false).Take(1).Subscribe(canRun);
        }
    }
}