using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;

public sealed class JobSidebarViewModel : ViewModelBase
{
    private readonly GlobalState _globalState;
    private readonly PageNavigation _navigationManager;
    private ObservableAsPropertyHelper<ImmutableList<JobSortOrderPair>>? _currentJobs;

    private ObservableAsPropertyHelper<JobSortOrderPair?>? _selectedValue;

    public JobSidebarViewModel(PageNavigation navigationManager, GlobalState globalState)
    {
        _navigationManager = navigationManager;
        _globalState = globalState;
        this.WhenActivated(Init);
    }

    public ReactiveCommand<Unit, Unit> NewJob { get; private set; } = null!;

    public ReactiveCommand<object, Unit> NewItemSelected { get; private set; } = null!;
    public JobSortOrderPair? SelectedValue => _selectedValue?.Value;
    public ImmutableList<JobSortOrderPair> CurrentJobs => _currentJobs?.Value ?? ImmutableList<JobSortOrderPair>.Empty;

    private IEnumerable<IDisposable> Init()
    {
        yield return _currentJobs = _globalState.Jobs.CurrentJobs
           .Select(Sort)
           .ObserveOn(RxApp.MainThreadScheduler).ToProperty(this, model => model.CurrentJobs);

        yield return _selectedValue = _globalState.Jobs.CurrentlySelectedPair
           .ObserveOn(RxApp.MainThreadScheduler)
           .ToProperty(this, p => p.SelectedValue);

        yield return NewJob = ReactiveCommand.Create(_navigationManager.NewJob, _globalState.IsOnline);

        yield return NewItemSelected = ReactiveCommand.Create<object, Unit>(
            o =>
            {
                if(o is not JobSortOrderPair pair || pair.Order.Id is null)
                    return Unit.Default;

                //Console.WriteLine($"Job Selected {o}");
                _globalState.Dispatch(new SelectNewPairAction(PairSelection.From(pair.Order.Id.Value)));

                return Unit.Default;
            },
            _globalState.IsOnline);
    }

    private ImmutableList<JobSortOrderPair> Sort(JobSortOrderPair[] unsorted)
        => unsorted
           .GroupBy(p => p.Info.Status)
           .OrderByDescending(g => g.Key)
           .SelectMany(StatusToPrioritySort)
           .ToImmutableList();

    private IEnumerable<JobSortOrderPair> StatusToPrioritySort(IGrouping<ProjectStatus, JobSortOrderPair> statusGroup)
    {
        if(statusGroup.Key is ProjectStatus.Entered or ProjectStatus.Pending)
            return statusGroup.GroupBy(s => s.Order.IsPriority)
               .OrderByDescending(g1 => g1.Key)
               .SelectMany(PriorityToSkipCountSort);

        return statusGroup.OrderBy(p => p.Info.Deadline?.Value ?? DateTimeOffset.MaxValue);
    }

    private IEnumerable<JobSortOrderPair> PriorityToSkipCountSort(IGrouping<bool, JobSortOrderPair> priorityGroup)
    {
        var temp = priorityGroup.OrderBy(p => p.Info.Deadline?.Value ?? DateTimeOffset.MaxValue).ToArray();
        for (var index = 0; index < temp.ToArray().Length; index++)
        {
            JobSortOrderPair orderPair = temp.ToArray()[index];

            if(orderPair.Order.SkipCount == 0)
                continue;

            SwapArray(orderPair, index, temp);
        }

        return temp;
    }

    private static void SwapArray(JobSortOrderPair orderPair, int index, JobSortOrderPair[] temp)
    {
        int skipCount = orderPair.Order.SkipCount;
        if(skipCount > index)
            skipCount = index;

        for (var i = 0; i < skipCount; i++)
            temp[index - i] = temp[index - i - 1];

        temp[index - skipCount] = orderPair;
    }
}