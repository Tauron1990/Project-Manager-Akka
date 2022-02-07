using System.Reactive.Linq;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Data.States;

public sealed class TaskState : StateBase, IProvideActionDispatcher
{
    private readonly ITaskManager _taskManager;
    private readonly IEventAggregator _aggregator;

    public TaskState(IStateFactory stateFactory, IStoreConfiguration storeConfiguration, ITaskManager taskManager, IEventAggregator aggregator) 
        : base(stateFactory)
    {
        _taskManager = taskManager;
        _aggregator = aggregator;
        storeConfiguration.RegisterForFhinising(this);

        ProviderFactory = () => stateFactory.NewComputed<PendingTask[]>(ComputeState);
    }

    public void StoreCreated(IActionDispatcher dispatcher)
    {
        dispatcher.ObservAction<DeleteTask>()
           .SelectMany(
                async dt => await _aggregator.IsSuccess(
                    () => TimeoutToken.WithDefault(CancellationToken.None, async t => await _taskManager.DeleteTask(dt.Task.Id, t))))
           .Subscribe();
    }

    public Func<IState<PendingTask[]>> ProviderFactory { get; }
    
    private async Task<PendingTask[]> ComputeState(IComputedState<PendingTask[]> unUsed, CancellationToken cancellationToken)
        => await _taskManager.GetTasks(cancellationToken);
}