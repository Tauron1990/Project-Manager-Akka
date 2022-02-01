using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Data.States;
using SimpleProjectManager.Client.Shared.CurrentJobs;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class JobPriorityViewModel : BlazorViewModel
{
    private readonly GlobalState _globalState;
    
    private IObservable<ImmutableList<JobSortOrderPair>> ActivePairs { get; }

    public ReactiveCommand<JobSortOrderPair?, Unit> GoUp { get; }

    public ReactiveCommand<JobSortOrderPair?, Unit> GoDown { get; }

    public ReactiveCommand<JobSortOrderPair?, Unit> Priorize { get; }

    public JobPriorityViewModel(IStateFactory stateFactory, GlobalState globalState)
        : base(stateFactory)
    {
        _globalState = globalState;

        ActivePairs = GetParameter<ImmutableList<JobSortOrderPair>>(nameof(JobPriorityControl.ActivePairs)).ToObservable();

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