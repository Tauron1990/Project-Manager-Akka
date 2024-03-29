﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public abstract class JobPriorityViewModelBase : ViewModelBase
{
    private readonly GlobalState _globalState;

    protected JobPriorityViewModelBase(GlobalState globalState)
    {
        _globalState = globalState;

        this.WhenActivated(Init);
    }

    private IObservable<ImmutableList<JobSortOrderPair>>? ActivePairs { get; set; }

    public ReactiveCommand<JobSortOrderPair?, Unit>? GoUp { get; private set; }

    public ReactiveCommand<JobSortOrderPair?, Unit>? GoDown { get; private set; }

    public ReactiveCommand<JobSortOrderPair?, Unit>? Priorize { get; private set; }

    private IEnumerable<IDisposable> Init()
    {
        ActivePairs = GetActivePairs();

        yield return GoUp = ReactiveCommand.Create(
            CreateExecute(info => new SetSortOrder(IgnoreIfEmpty: false, SortOrder: info.Order.Increment())),
            CreateCanExecute(_globalState, (pairs, pair) => pairs[0] != pair));

        yield return GoDown = ReactiveCommand.Create(
            CreateExecute(info => new SetSortOrder(IgnoreIfEmpty: false, SortOrder: info.Order.Decrement())),
            CreateCanExecute(_globalState, (pairs, pair) => pairs[^1] != pair));

        yield return Priorize = ReactiveCommand.Create(
            CreateExecute(info => new SetSortOrder(IgnoreIfEmpty: false, SortOrder: info.Order.Priority())),
            CreateCanExecute(_globalState, (_, pair) => !pair.Order.IsPriority));

        Action<JobSortOrderPair?> CreateExecute(Func<JobSortOrderPair, SetSortOrder> executor)
            => i =>
               {
                   if(i is null) return;

                   _globalState.Dispatch(executor(i));
               };

        IObservable<bool> CreateCanExecute(GlobalState state, Func<ImmutableList<JobSortOrderPair>, JobSortOrderPair, bool> predicate)
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

    protected abstract IObservable<ImmutableList<JobSortOrderPair>> GetActivePairs();
}