using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleProjectManager.Client.Shared.Data.States.Actions;
using SimpleProjectManager.Client.Shared.Services;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Tauron;
using Tauron.Applicarion.Redux;
using Tauron.Applicarion.Redux.Configuration;

namespace SimpleProjectManager.Client.Shared.Data.States;

public sealed class TaskState : StateBase, IProvideActionDispatcher, IStoreInitializer
{
    private readonly ITaskManager _taskManager;
    private readonly IMessageMapper _aggregator;

    public TaskState(IStateFactory stateFactory, ITaskManager taskManager, IMessageMapper aggregator) 
        : base(stateFactory)
    {
        _taskManager = taskManager;
        _aggregator = aggregator;

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

    public void RunConfig(IStoreConfiguration configuration)
        => configuration.RegisterForFhinising(this);
}