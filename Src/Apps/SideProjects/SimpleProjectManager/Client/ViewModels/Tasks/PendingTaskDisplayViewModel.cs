using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using ReactiveUI;
using SimpleProjectManager.Client.Shared.Tasks;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Fusion;
using Tauron;
using Tauron.Application;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.ViewModels;

public sealed class PendingTaskDisplayViewModel : BlazorViewModel
{
    private readonly BehaviorSubject<bool> _canRun = new(true);

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    public PendingTaskDisplayViewModel(IStateFactory stateFactory, ITaskManager taskManager, IEventAggregator aggregator)
        : base(stateFactory)
    {
        var taskState = GetParameter<PendingTask?>(nameof(PendingTaskDisplay.PendingTask));
        _canRun.DisposeWith(this);

        Cancel = ReactiveCommand.CreateFromTask(
                async () =>
                {
                    var task = taskState.ValueOrDefault;

                    if (task == null) return Unit.Default;
                    
                    await aggregator.IsSuccess(() => TimeoutToken.WithDefault(
                                                   t => taskManager.DeleteTask(task.Id, t)));
                    return Unit.Default;
                },
                _canRun.CombineLatest(taskState.ToObservable().Select(pt => pt is not null))
                   .Select(i => i.First && i.Second))
           .DisposeWith(this);

        Cancel.Select(_ => false).Take(1).Subscribe(_canRun).DisposeWith(this);
    }
}