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
    private readonly GlobalState _globalState;
    private readonly IMessageDispatcher _messageDispatcher;

    public PendingTaskDisplayViewModel(GlobalState globalState, IStateFactory stateFactory, IMessageDispatcher messageDispatcher)
    {
        _globalState = globalState;
        _messageDispatcher = messageDispatcher;
        PendingTask = stateFactory.NewMutable<PendingTask?>();
        
        this.WhenActivated(Init);
    }

    private IEnumerable<IDisposable> Init()
    {
        BehaviorSubject<bool>? canRun;
        yield return canRun = new BehaviorSubject<bool>(value: true);
        yield return Cancel = ReactiveCommand.Create(
            () =>
            {
                PendingTask? task = PendingTask.ValueOrDefault;

                if(task is null) return;

                _globalState.Dispatch(new DeleteTask(task));
            },
            //PendingTask.ToObservable().Select(pt => pt is not null)
            canRun.CombineLatest(PendingTask.ToObservable(_messageDispatcher.IgnoreErrors()).Select(pt => pt is not null))
               .Select(i => i.First && i.Second)
               .AndIsOnline(_globalState.OnlineMonitor)
        );

        yield return Cancel.Select(_ => false).Take(1).Subscribe(canRun);
    }
    
    public IMutableState<PendingTask?> PendingTask { get; }

    public ReactiveCommand<Unit, Unit>? Cancel { get; private set; }
}