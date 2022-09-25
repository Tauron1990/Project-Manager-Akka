using System.Collections.Immutable;
using SimpleProjectManager.Client.Shared;
using SimpleProjectManager.Client.Shared.CurrentJobs;
using SimpleProjectManager.Client.Shared.Data;
using SimpleProjectManager.Client.Shared.Data.States;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Client.Shared.ViewModels.CurrentJobs;
using Stl.Fusion;
using Tauron;
using Tauron.Application.Blazor.Parameters;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class JobPriorityViewModel : JobPriorityViewModelBase, IParameterUpdateable
{
    private readonly IStateFactory _factory;
    private readonly IMessageDispatcher _messageDispatcher;

    public JobPriorityViewModel(GlobalState globalState, IStateFactory factory, IMessageDispatcher messageDispatcher) 
        : base(globalState)
    {
        _factory = factory;
        _messageDispatcher = messageDispatcher;
    }

    protected override IObservable<ImmutableList<JobSortOrderPair>> GetActivePairs()
        => Updater
           .Register<ImmutableList<JobSortOrderPair>>(nameof(JobPriorityControl.ActivePairs), _factory)
           .ToObservable(_messageDispatcher.PropagateErrors());

    public ParameterUpdater Updater { get; } = new();
}