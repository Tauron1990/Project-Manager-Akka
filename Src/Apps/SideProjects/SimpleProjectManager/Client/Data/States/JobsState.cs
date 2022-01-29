using System.Collections.Immutable;
using System.Reactive.Linq;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Validators;
using Stl.Fusion;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.States;

public sealed class JobsState : StateBase<InternalJobData>
{
    private readonly UpdateProjectCommandValidator _upodateProjectValidator = new();
    private readonly ProjectNameValidator _nameValidator = new();
    
    private readonly IJobDatabaseService _service;
    private readonly IEventAggregator _eventAggregator;
    
    public JobsState(IStoreConfiguration storeConfiguration, IStateFactory stateFactory)
        : base(storeConfiguration, stateFactory)
    {
        _service = stateFactory.Services.GetRequiredService<IJobDatabaseService>();
        _eventAggregator = stateFactory.Services.GetRequiredService<IEventAggregator>();
    }

    protected override IStateConfiguration<InternalJobData> ConfigurateState(ISourceConfiguration<InternalJobData> configuration)
    {
        return configuration.FromCacheAndServer<(JobInfo[], SortOrder[])>(
                async token =>
                {
                    var jobs = await _service.GetActiveJobs(token);
                    var order = await _service.GetSortOrders(token);

                    return (jobs, order);
                },
                (originalData, serverData) =>
                {
                    var (jobs, orders) = serverData;
                    var pairs = jobs.Select(j => new JobSortOrderPair(orders.First(o => o.Id == j.Project), j)).ToArray();

                    var selection = originalData.CurrentSelected;
                    // ReSharper disable once AccessToModifiedClosure
                    if (selection.Pair != null && pairs.All(s => s.Info.Project != selection.Pair.Info.Project))
                        selection = new CurrentSelected(null, null);

                    return originalData with { CurrentJobs = pairs, CurrentSelected = selection };
                })
           .ApplyReducers(
                f => f.On<SelectNewPairAction>(JobDataPatcher.ReplaceSlected))
           .ApplyRequests(
                requestFactory =>
                    requestFactory.OnTheFlyUpdate(
                            JobDataSelectors.CurrentSelected,
                            (cancel, source) => JobDataRequests.FetchjobData(cancel, source, _service),
                            JobDataPatcher.ReplaceSelected)
                       .AddRequest<SetSortOrder>(_service.ChangeOrder!, JobDataPatcher.PatchSortOrder));
    }

    protected override void PostConfiguration(IRootStoreState<InternalJobData> state)
    {
        CurrentlySelectedData = state.Select(jobData => jobData.CurrentSelected.JobData);
        CurrentlySelectedPair = state.Select(jobData => jobData.CurrentSelected.Pair);
        CurrentJobs = state.Select(jobData => jobData.CurrentJobs);
        ActiveJobsCount = FromServer(_service.CountActiveJobs);
    }
    
    public IObservable<JobSortOrderPair[]> CurrentJobs { get; private set; } = Observable.Empty<JobSortOrderPair[]>();

    public IObservable<JobData?> CurrentlySelectedData { get; private set; } = Observable.Empty<JobData>();

    public IObservable<JobSortOrderPair?> CurrentlySelectedPair { get; private set; } = Observable.Empty<JobSortOrderPair>();

    public IObservable<long> ActiveJobsCount { get; private set; } = Observable.Empty<long>();

    public string ValidateProjectName(string? arg)
    {
        var name = new ProjectName(arg ?? string.Empty);
        var result = _nameValidator.Validate(name);
        
        return result.IsValid ? string.Empty : string.Join(", ", result.Errors.Select(err => err.ErrorMessage));
    }
    
    public IObservable<JobEditorData?> GetJobEditorData(IObservable<string> idObservable)
        => idObservable.SelectMany
        (
            id => TimeoutToken.WithDefault(
                CancellationToken.None,
                async token => string.IsNullOrWhiteSpace(id) ? null : new JobEditorData(await _service.GetJobData(new ProjectId(id), token))).AsTask()
        );

    public void NewSelection(JobSortOrderPair? newSelection)
        => Dispatch(new SelectNewPairAction(newSelection));

    public void SetNewSortOrder(SetSortOrder order)
        => Dispatch(order);
    
    public async Task CommitJobData(JobEditorCommit newData, Action onCompled)
    {
        if (newData.JobData.OldData == null)
        {
            _eventAggregator.PublishError("Keine Original Daten zur verfügung gestellt");
            return;
        }

        var command = UpdateProjectCommand.Create(newData.JobData.NewData, newData.JobData.OldData);
        var validationResult = await _upodateProjectValidator.ValidateAsync(command);

        if (validationResult.IsValid)
        {
            if (await _eventAggregator.IsSuccess(() => TimeoutToken.WithDefault(default, t => _service.UpdateJobData(command, t)))
             && await _eventAggregator.IsSuccess(async () => await newData.Upload())) 
                onCompled();
        }
        else
        {
            var err = string.Join(", ", validationResult.Errors.Select(f => f.ErrorMessage));
            _eventAggregator.PublishWarnig(err);
        }
    }
    
    public string? TryExtrectName(string name)
    {
        var upperName = name.ToUpper().AsSpan();

        var index = upperName.IndexOf("BM");

        if (index == -1) return null;

        while (upperName.Length >= 10)
        {
            upperName = upperName[index..];
            if(upperName.Length != 10) break;

            if (AllDigit(upperName[2..2]) &&
                upperName[4] == '_' &&
                AllDigit(upperName[5..10]))
            {
                return upperName[..10].ToString();
            }
        }

        return null;
    }

    private bool AllDigit(in ReadOnlySpan<char> input)
    {
        foreach (var t in input)
        {
            if(char.IsDigit(t)) continue;

            return false;
        }

        return true;
    }
    
    public JobEditorCommit CreateNewJobData(JobEditorData editorData, Func<Task<string>> start)
    {
        var data = editorData.OriginalData;
        if (data != null)
        {
            data = data with
                   {
                       JobName = new ProjectName(editorData.JobName ?? string.Empty),
                       Status = editorData.Status,
                       Deadline = ProjectDeadline.FromDateTime(editorData.Deadline),
                       Ordering = GetOrdering(data.Id)
                   };
        }
        else
        {
            var name = new ProjectName(editorData.JobName ?? string.Empty);
            var id = ProjectId.For(name);
            data = new JobData(id,  name, editorData.Status, GetOrdering(id), 
                ProjectDeadline.FromDateTime(editorData.Deadline), ImmutableList<ProjectFileId>.Empty);
        }

        SortOrder? GetOrdering(ProjectId id)
        {
            if (editorData.Ordering != null)
            {
                return data?.Ordering == null 
                    ? new SortOrder(id, editorData.Ordering.Value, false) 
                    : data.Ordering.WithCount(editorData.Ordering.Value);
            }

            return data?.Ordering;
        }

        return new JobEditorCommit(new JobEditorPair<JobData>(data, editorData.OriginalData), start);
    }
}