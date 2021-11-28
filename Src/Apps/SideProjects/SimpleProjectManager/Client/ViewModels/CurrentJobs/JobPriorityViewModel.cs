using System.Collections.Immutable;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.CurrentJobs;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class JobPriorityViewModel : BlazorViewModel
{
    private readonly JobsViewModel _model;

    public IObservable<ImmutableList<JobSortOrderPair>> ActivePairs { get; }

    public ReactiveCommand<JobSortOrderPair?, bool> GoUp { get; }

    public ReactiveCommand<JobSortOrderPair?, bool> GoDown { get; }

    public ReactiveCommand<JobSortOrderPair?, bool> Priorize { get; }

    public JobPriorityViewModel(IStateFactory stateFactory, IJobDatabaseService databaseService, IEventAggregator aggregator, JobsViewModel model)
        : base(stateFactory)
    {
        if (stateFactory == null) throw new ArgumentNullException(nameof(stateFactory));
        if (databaseService == null) throw new ArgumentNullException(nameof(databaseService));
        if (aggregator == null) throw new ArgumentNullException(nameof(aggregator));

        _model = model ?? throw new ArgumentNullException(nameof(model));

        ActivePairs = GetParameter<ImmutableList<JobSortOrderPair>>(nameof(JobPriorityControl.ActivePairs)).ToObservable();
        
        GoUp = ReactiveCommand.CreateFromObservable(CreateExecute(
            async info => await aggregator.IsSuccess(
                () => TimeoutToken.WithDefault(default,
                    token => databaseService.ChangeOrder(new SetSortOrder(info.Order.Increment()), token)))),
            CreateCanExecute((pairs, pair) => pairs[0] != pair))
           .DisposeWith(this);

        GoDown = ReactiveCommand.CreateFromObservable(CreateExecute(
            async info => await aggregator.IsSuccess(
                () => TimeoutToken.WithDefault(default,
                    token => databaseService.ChangeOrder(new SetSortOrder(info.Order.Decrement()), token)))),
            CreateCanExecute((pairs, pair) => pairs.Last() != pair))
           .DisposeWith(this);

        Priorize = ReactiveCommand.CreateFromObservable(CreateExecute(
            async info => await aggregator.IsSuccess(
                () => TimeoutToken.WithDefault(default,
                    token => databaseService.ChangeOrder(new SetSortOrder(info.Order.Decrement()), token)))),
            CreateCanExecute((_, pair) => !pair.Order.IsPriority))
           .DisposeWith(this);
    }

    private static Func<JobSortOrderPair?, IObservable<bool>> CreateExecute(Func<JobSortOrderPair, Task<bool>> executor)
        => i => from info in Observable.Return(i)
                where info is not null
                from result in executor(info)
                select result;

    private IObservable<bool> CreateCanExecute(Func<ImmutableList<JobSortOrderPair>, JobSortOrderPair, bool> predicate)
        => 
        (
            from activePair in ActivePairs
            from jobSortOrderPair in _model.CurrentInfo
            select activePair != null 
                && jobSortOrderPair != null 
                && activePair.Contains(jobSortOrderPair) 
                && predicate(activePair, jobSortOrderPair)
        ).StartWith(false);
}