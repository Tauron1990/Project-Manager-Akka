using System.Collections.Immutable;
using System.Reactive;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.CurrentJobs;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public class JobSidebarViewModel : StatefulViewModel<ImmutableList<JobSortOrderPair>>
{
    private readonly IJobDatabaseService _databaseService;

    public IState<JobInfo[]?> CurrentJobs { get; }

    public ReactiveCommand<Unit, Unit> NewJob { get; }

    public ReactiveCommand<object, Unit> NewItemSelected { get; }

    private readonly ObservableAsPropertyHelper<JobSortOrderPair?> _selectedValue;
    public JobSortOrderPair? SelectedValue => _selectedValue.Value;
    
    public JobSidebarViewModel(IStateFactory stateFactory, JobsViewModel jobsViewModel, IJobDatabaseService databaseService, 
        PageNavigation navigationManager) 
        : base(stateFactory)
    {
        _databaseService = databaseService;
        
        CurrentJobs = GetParameter<JobInfo[]?>(nameof(JobSideBar.CurrentJobs));
        _selectedValue = jobsViewModel.CurrentInfo.ToProperty(this, p => p.SelectedValue)
           .DisposeWith(this);
        
        NewJob = ReactiveCommand.Create(navigationManager.NewJob);
        NewItemSelected = ReactiveCommand.Create<object, Unit>(
            o =>
            {
                if(o is JobSortOrderPair pair)
                    jobsViewModel.Publish(pair);
                
                return Unit.Default;
            });
    }
    
    
    protected override async Task<ImmutableList<JobSortOrderPair>> ComputeState(CancellationToken cancellationToken)
    {
        try
        {
            var list = new List<JobSortOrderPair>();

            var currentJobs = await CurrentJobs.Use(cancellationToken);
            
    // ReSharper disable once InvertIf
            if (currentJobs != null)
            {
                var activeJobs = currentJobs.ToDictionary(c => c.Project);
                foreach (var sortOrder in await _databaseService.GetSortOrders(cancellationToken))
                {
                    if (activeJobs.Remove(sortOrder.Id, out var data))
                        list.Add(new JobSortOrderPair(sortOrder, data));
                }

                list.AddRange(activeJobs.Select(job => new JobSortOrderPair(new SortOrder(job.Key, 0, false), job.Value)));
            }

            return list
                //.OrderByDescending(j => j, JobSortOrderPairComparer.Comp)
                .GroupBy(p => p.Info.Status)
                .OrderByDescending(g => g.Key)
                .SelectMany(StatusToPrioritySort)
                .ToImmutableList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

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