using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using SimpleProjectManager.Client.Shared.Data.JobEdit;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Client.Shared.Data.States.Data;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Validators;
using Stl.Fusion;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Operations;

#pragma warning disable GU0011

namespace SimpleProjectManager.Client.Shared.Data.States.JobState;

public sealed partial class JobsState : StateBase<InternalJobData>
{
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly ProjectNameValidator _nameValidator = new();

    private readonly IJobDatabaseService _service;
    private readonly UpdateProjectCommandValidator _upodateProjectValidator = new();

    public JobsState(IStateFactory stateFactory, IJobDatabaseService jobDatabaseService, IMessageDispatcher messageDispatcher)
        : base(stateFactory)
    {
        _service = jobDatabaseService;
        _messageDispatcher = messageDispatcher;
    }

    public IObservable<bool> IsLoaded { get; private set; } = Observable.Empty<bool>();

    public IObservable<JobSortOrderPair[]> CurrentJobs { get; private set; } = Observable.Empty<JobSortOrderPair[]>();

    public IObservable<JobData?> CurrentlySelectedData { get; private set; } = Observable.Empty<JobData>();

    public IObservable<JobSortOrderPair?> CurrentlySelectedPair { get; private set; } = Observable.Empty<JobSortOrderPair>();

    public IObservable<ActiveJobs> ActiveJobsCount { get; private set; } = Observable.Empty<ActiveJobs>();

    protected override IStateConfiguration<InternalJobData> ConfigurateState(ISourceConfiguration<InternalJobData> configuration)
    {
        return ConfigurateEditor
            (
                configuration.FromCacheAndServer<(Jobs, SortOrders)>(
                    async token =>
                    {
                        Jobs jobs = await _service.GetActiveJobs(token).ConfigureAwait(false);
                        SortOrders order = await _service.GetSortOrders(token).ConfigureAwait(false);

                        return (jobs, order);
                    },
                    (originalData, serverData) =>
                    {
                        try
                        {
                            (var jobs, SortOrders orders) = serverData;
                            var pairs = jobs.JobInfos.Select(j => new JobSortOrderPair(orders.OrdersList.First(o => o.Id == j.Project), j)).ToArray();

                            CurrentSelected? selection = originalData.CurrentSelected;
                            // ReSharper disable once AccessToModifiedClosure
                            if(selection?.Pair is not null && pairs.All(s => s.Info.Project != selection.Pair?.Info.Project))
                                selection = new CurrentSelected(null, null);

                            // ReSharper disable once WithExpressionModifiesAllMembers
                            return originalData with { IsLoaded = true, CurrentJobs = pairs, CurrentSelected = selection };
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);

                            throw;
                        }
                    })
            ).ApplyReducers(
                f => f.On<SelectNewPairAction>(JobDataPatcher.ReplaceSlected))
           .ApplyRequests(
                requestFactory =>
                {
                    requestFactory.OnTheFlyUpdate(
                        JobDataSelectors.CurrentSelected,
                        (cancel, source) => JobDataRequests.FetchjobData(source, _service, cancel),
                        JobDataPatcher.ReplaceSelected);
                    requestFactory.AddRequest<InternalJobData, SetSortOrder>(_service.ChangeOrder, JobDataPatcher.PatchSortOrder);
                });
    }

    protected override void PostConfiguration(IRootStoreState<InternalJobData> state)
    {
        CurrentlySelectedData = state.Select(jobData => jobData.CurrentSelected?.JobData);
        CurrentlySelectedPair = state.Select(jobData => jobData.CurrentSelected?.Pair);
        CurrentJobs = state.Select(jobData => jobData.CurrentJobs);
        IsLoaded = state.Select(s => s.IsLoaded);
        CurrentJobs.Subscribe();
        ActiveJobsCount = FromServer(_service.CountActiveJobs, ActiveJobs.From(0));
    }
}

public partial class JobsState
{
    private IStateConfiguration<InternalJobData> ConfigurateEditor(IStateConfiguration<InternalJobData> configuration)
        => configuration.ApplyRequests(
            f => { f.AddRequest(JobDataRequests.PostJobCommit(_service, _messageDispatcher), JobDataPatcher.PatchEditorCommit); });

    public IEnumerable<string> ValidateProjectName(string? arg)
    {
        var name = new ProjectName(arg ?? string.Empty);
        ValidationResult? result = _nameValidator.Validate(name);

        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(err => err.ErrorMessage).ToArray();
    }

    public IObservable<JobEditorData?> GetJobEditorData(IObservable<string> idObservable)
        => idObservable.SelectMany
        (
            id => TimeoutToken.WithDefault(
                CancellationToken.None,
                async token =>
                    string.IsNullOrWhiteSpace(id)
                        ? null
                        : new JobEditorData(await _service.GetJobData(new ProjectId(id), token).ConfigureAwait(false))).AsTask()
        );

    public async Task CommitJobData(JobEditorCommit newData, Action onCompled)
    {
        if(newData.JobData.OldData is null)
        {
            _messageDispatcher.PublishError("Keine Original Daten zur verfügung gestellt");

            return;
        }

        var command = UpdateProjectCommand.Create(newData.JobData.NewData, newData.JobData.OldData);
        ValidationResult? validationResult = await _upodateProjectValidator.ValidateAsync(command).ConfigureAwait(false);

        if(validationResult.IsValid)
        {
            if(await _messageDispatcher.IsSuccess(() => TimeoutToken.WithDefault(default, t => _service.UpdateJobData(command, t))).ConfigureAwait(false)
            && await _messageDispatcher.IsSuccess(async () => await newData.Upload().ConfigureAwait(false)).ConfigureAwait(false))
                onCompled();
        }
        else
        {
            string err = string.Join(", ", validationResult.Errors.Select(f => f.ErrorMessage));
            _messageDispatcher.PublishWarnig(err);
        }
    }

    public static string? TryExtrectName(FileName name)
    {
        var upperName = name.Value.ToUpper(CultureInfo.CurrentCulture).AsSpan();

        #pragma warning disable EPS06
        int index = upperName.IndexOf("BM", StringComparison.CurrentCulture);
        #pragma warning restore EPS06

        if(index == -1) return null;

        while (upperName.Length >= 10)
        {
            upperName = upperName[index..];

            if(upperName.Length != 10) break;

            if(AllDigit(upperName[2..2]) &&
               upperName[4] == '_' &&
               AllDigit(upperName[5..10]))
                return upperName[..10].ToString();
        }

        return null;
    }
    #pragma warning disable EPS02
    private static bool AllDigit(in ReadOnlySpan<char> input)
    {
        foreach (char t in input)
        {
            if(char.IsDigit(t)) continue;

            return false;
        }

        return true;
    }

    public JobEditorCommit CreateNewJobData(JobEditorData editorData, Func<Task<SimpleResult>> start)
    {
        JobData? data = editorData.OriginalData;
        if(data != null)
        {
            data = data with
                   {
                       JobName = editorData.JobName ?? ProjectName.Empty,
                       Status = editorData.Status,
                       Deadline = ProjectDeadline.FromDateTime(editorData.Deadline),
                       Ordering = GetOrdering(data.Id),
                   };
        }
        else
        {
            ProjectName name = editorData.JobName ?? ProjectName.Empty;
            ProjectId id = ProjectId.For(name);
            data = new JobData(
                id,
                name,
                editorData.Status,
                GetOrdering(id),
                ProjectDeadline.FromDateTime(editorData.Deadline),
                ImmutableList<ProjectFileId>.Empty);
        }

        SortOrder? GetOrdering(ProjectId id)
        {
            if(editorData.Ordering != null)
                return data?.Ordering is null
                    ? new SortOrder
                      {
                          Id = id,
                          SkipCount = editorData.Ordering.Value,
                          IsPriority = false,
                      }
                    : data.Ordering.WithCount(editorData.Ordering.Value);

            return data?.Ordering;
        }

        return new JobEditorCommit(new JobEditorPair<JobData>(data, editorData.OriginalData), start);
    }
}