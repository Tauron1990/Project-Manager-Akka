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

public sealed class TaskState : StateBase, IProvideRootStore, IStoreInitializer
{
    private readonly IMessageDispatcher _aggregator;
    private readonly ITaskManager _taskManager;

    public TaskState(IStateFactory stateFactory, ITaskManager taskManager, IMessageDispatcher aggregator)
        : base(stateFactory)
    {
        _taskManager = taskManager;
        _aggregator = aggregator;

        ProviderFactory = () => stateFactory.NewComputed<TaskList>(ComputeState);
    }

    public Func<IState<TaskList>> ProviderFactory { get; }


    public void StoreCreated(IRootStore dispatcher)
    {
        dispatcher.ObservAction<DeleteTask>()
           .SelectMany(
                async dt => await _aggregator.IsSuccess(
                    () => TimeoutToken.WithDefault(
                        CancellationToken.None, 
                        async t => await _taskManager.DeleteTask(dt.Task.Id, t).ConfigureAwait(false)))
                   .ConfigureAwait(false))
           .Subscribe();
    }

    public void RunConfig(IStoreConfiguration configuration)
        => configuration.RegisterForFhinising(this);

    private async Task<TaskList> ComputeState(IComputedState<TaskList> unUsed, CancellationToken cancellationToken)
        => await _taskManager.GetTasks(cancellationToken).ConfigureAwait(false);
}