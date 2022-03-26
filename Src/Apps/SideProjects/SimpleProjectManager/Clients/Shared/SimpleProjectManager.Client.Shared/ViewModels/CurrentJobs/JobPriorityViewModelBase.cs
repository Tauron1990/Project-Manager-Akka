using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Shared.Services;
using Tauron;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public abstract class JobPriorityViewModelBase : ViewModelBase
{
    private readonly GlobalState _globalState;
    
    private IObservable<ImmutableList<JobSortOrderPair>> ActivePairs { get; }

    public ReactiveCommand<JobSortOrderPair?, Unit> GoUp { get; }

    public ReactiveCommand<JobSortOrderPair?, Unit> GoDown { get; }

    public ReactiveCommand<JobSortOrderPair?, Unit> Priorize { get; }

    protected JobPriorityViewModelBase(GlobalState globalState, IObservable<ImmutableList<JobSortOrderPair>> activePairs)
    {
        _globalState = globalState;

        ActivePairs = activePairs;

        GoUp = ReactiveCommand.Create(
                CreateExecute(info => new SetSortOrder(false, info.Order.Increment())),
                CreateCanExecute(globalState, (pairs, pair) => pairs[0] != pair))
           .DisposeWith(this);

        GoDown = ReactiveCommand.Create(
                CreateExecute(info => new SetSortOrder(false, info.Order.Decrement())),
                CreateCanExecute(globalState, (pairs, pair) => pairs.Last() != pair))
           .DisposeWith(this);

        Priorize = ReactiveCommand.Create(
                CreateExecute(info => new SetSortOrder(false, info.Order.Priority())),
                CreateCanExecute(globalState, (_, pair) => !pair.Order.IsPriority))
           .DisposeWith(this);
    }

    private Action<JobSortOrderPair?> CreateExecute(Func<JobSortOrderPair, SetSortOrder> executor)
        => i =>
           {
               if (i is null) return;
               _globalState.Dispatch(executor(i));     
           };

    private IObservable<bool> CreateCanExecute(GlobalState state, Func<ImmutableList<JobSortOrderPair>, JobSortOrderPair, bool> predicate)
        => 
        (
            from info in ActivePairs.CombineLatest(
                _globalState.IsOnline, 
                _globalState.Jobs.CurrentlySelectedPair,
                (pairs, online, selected) => (pairs, online, selected))
            
            select info.online
                && info.pairs is not null 
                && info.selected is not null 
                && info.pairs.Contains(info.selected) 
                && predicate(info.pairs, info.selected)
        ).StartWith(false).AndIsOnline(state.OnlineMonitor);
}