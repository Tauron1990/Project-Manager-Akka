﻿using System.Collections.Immutable;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using SimpleProjectManager.Client.Data;
using SimpleProjectManager.Client.Data.States;
using SimpleProjectManager.Shared;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public class JobSidebarViewModel : BlazorViewModel
{
    public ReactiveCommand<Unit, Unit> NewJob { get; }

    public ReactiveCommand<object, Unit> NewItemSelected { get; }

    private readonly ObservableAsPropertyHelper<JobSortOrderPair?> _selectedValue;
    public JobSortOrderPair? SelectedValue => _selectedValue.Value;

    private readonly ObservableAsPropertyHelper<ImmutableList<JobSortOrderPair>> _currentJobs;
    public ImmutableList<JobSortOrderPair> CurrentJobs => _currentJobs.Value;
    
    public JobSidebarViewModel(IStateFactory stateFactory, PageNavigation navigationManager, GlobalState globalState) 
        : base(stateFactory)
    {
        _currentJobs = globalState.JobsState.CurrentJobs.Select(Sort).ToProperty(this, model => model.CurrentJobs).DisposeWith(this);
        _selectedValue = globalState.JobsState.CurrentlySelectedPair.ToProperty(this, p => p.SelectedValue).DisposeWith(this);
        
        NewJob = ReactiveCommand.Create(navigationManager.NewJob, globalState.IsOnline);
        NewItemSelected = ReactiveCommand.Create<object, Unit>(
            o =>
            {
                if(o is JobSortOrderPair pair)
                    globalState.JobsState.NewSelection(pair);
                
                return Unit.Default;
            }, globalState.IsOnline);
    }

    private ImmutableList<JobSortOrderPair> Sort(JobSortOrderPair[] unsorted)
        => unsorted
           .GroupBy(p => p.Info.Status)
           .OrderByDescending(g => g.Key)
           .SelectMany(StatusToPrioritySort)
           .ToImmutableList();
    
    private IEnumerable<JobSortOrderPair> StatusToPrioritySort(IGrouping<ProjectStatus, JobSortOrderPair> statusGroup)
    {
        if (statusGroup.Key is ProjectStatus.Entered or ProjectStatus.Pending)
        {
            return statusGroup.GroupBy(s => s.Order.IsPriority)
                .OrderByDescending(g1 => g1.Key)
                .SelectMany(PriorityToSkipCountSort);
        }

        return statusGroup.OrderBy(p => p.Info.Deadline?.Value ?? DateTimeOffset.MaxValue);
    }

    private IEnumerable<JobSortOrderPair> PriorityToSkipCountSort(IGrouping<bool, JobSortOrderPair> priorityGroup)
    {
        var temp = priorityGroup.OrderBy(p => p.Info.Deadline?.Value ?? DateTimeOffset.MaxValue).ToArray();
        for (var index = 0; index < temp.ToArray().Length; index++)
        {
            var orderPair = temp.ToArray()[index];

            if (orderPair.Order.SkipCount == 0)
                continue;

            SwapArray(orderPair, index, temp);
        }

        return temp;
    }

    private static void SwapArray(JobSortOrderPair orderPair, int index, JobSortOrderPair[] temp)
    {
        var skipCount = orderPair.Order.SkipCount;
        if (skipCount > index)
            skipCount = index;
        
        for (var i = 0; i < skipCount; i++)
        {
            temp[index - i] = temp[index - i - 1];
        }
        
        temp[index - skipCount] = orderPair;
    }
}