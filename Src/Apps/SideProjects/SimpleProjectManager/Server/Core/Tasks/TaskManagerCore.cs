using System.Collections.Concurrent;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akka.DependencyInjection;
using Akkatecture.Jobs;
using Akkatecture.Jobs.Commands;
using SimpleProjectManager.Server.Data;
using SimpleProjectManager.Server.Data.Data;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Tasks;

#pragma warning disable EX006

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed class TaskManagerCore
{
    private readonly ActorSystem _actorSystem;
    private readonly IEventAggregator _aggregator;

    private readonly ConcurrentDictionary<string, TaskManagerJobId> _autoCancelDely = new();
    private readonly CriticalErrorHelper _criticalErrors;
    private readonly MappingDatabase<DbTaskManagerEntry, TaskManagerEntry> _entrys;
    private readonly ConcurrentDictionary<string, IActorRef> _jobManagers = new();
    private readonly ConcurrentBag<RegisterJobType> _registrations = new();
    private IActorRef _autoRemoveManager = Nobody.Instance;

    public TaskManagerCore(
        IInternalDataRepository repository, ActorSystem actorSystem, ICriticalErrorService criticalErrorService,
        ILogger<TaskManagerCore> logger, IEventAggregator aggregator)
    {
        _actorSystem = actorSystem;
        _aggregator = aggregator;
        _criticalErrors = new CriticalErrorHelper(nameof(TaskManagerCore), criticalErrorService, logger);
        _entrys = new MappingDatabase<DbTaskManagerEntry, TaskManagerEntry>(
            repository.Databases.TaskManagerEntrys,
            repository.Databases.Mapper);

        InitAutoDelete();
    }

    private void InitAutoDelete()
    {
        _autoRemoveManager = _actorSystem.ActorOf(Props.Create(() => new TaskManagerDeleteEntryManager(_entrys, _criticalErrors, _aggregator)));
        _actorSystem.ActorOf(
            d =>
            {
                d.OnPreStart = context => context.System.EventStream.Subscribe(context.Self, typeof(ISchedulerEvent));
                d.OnPostStop = context => context.System.EventStream.Unsubscribe(context.Self, typeof(ISchedulerEvent));

                d.Receive<ISchedulerEvent>(HandleSchedulerEvent);
            });
    }

    private void HandleSchedulerEvent(ISchedulerEvent evt, IActorContext context)
    {
        switch (evt.GetEventType())
        {
            case JobEventType.Cancel:
                return;
            case JobEventType.Finish:
                ProcessFinish(evt);

                break;
            case JobEventType.Schedule:
                if(_autoCancelDely.TryRemove(evt.GetJobId(), out TaskManagerJobId? cancelId))
                    _autoRemoveManager.Tell(new Cancel<TaskManagerDeleteEntry, TaskManagerJobId>(cancelId));

                break;
        }
    }

    private void ProcessFinish(ISchedulerEvent evt)
    {
        if(evt is not ISchedulerEvent<TaskManagerDeleteEntry, TaskManagerJobId>)
        {
            _autoCancelDely.AddOrUpdate(
                evt.GetJobId(),
                jobId =>
                {
                    TaskManagerJobId id = TaskManagerJobId.New;
                    _autoRemoveManager.Tell(
                        new Schedule<TaskManagerDeleteEntry, TaskManagerJobId>(
                            id,
                            new TaskManagerDeleteEntry(jobId),
                            DateTime.Now + TimeSpan.FromMinutes(5)));

                    return id;
                },
                (_, id) => id);
        }
        else
        {
            var id = new TaskManagerJobId(evt.GetJobId());
            var toRemove = _autoCancelDely.FirstOrDefault(e => e.Value == id);

            if(string.IsNullOrWhiteSpace(toRemove.Key)) return;

            _autoCancelDely.TryRemove(toRemove.Key, out _);
        }
    }

    private IActorRef GetJobManager(RegisterJobType registration)
        => _jobManagers.GetOrAdd(
            registration.Id,
            static (_, fac) => fac.System.ActorOf(fac.JobManagerFactory()),
            (registration.JobManagerFactory, System: _actorSystem));

    public TaskManagerCore Register(RegisterJobType registerJobType)
    {
        _registrations.Add(registerJobType);

        return this;
    }

    public TaskManagerCore Register<TJobManager, TJobScheduler, TJobRunner, TJob, TId>(string id, IDependencyResolver resolver)
        where TJobManager : JobManager<TJobScheduler, TJobRunner, TJob, TId>
        where TJob : IJob
        where TId : IJobId
        where TJobRunner : JobRunner<TJob, TId>
        where TJobScheduler : JobScheduler<TJobScheduler, TJob, TId>
    {
        Register(RegisterJobType.Create<TJobManager, TJobScheduler, TJobRunner, TJob, TId>(id, resolver));

        return this;
    }


    public async ValueTask<IOperationResult> AddNewTask(AddTaskCommand command, CancellationToken token)
        => await _criticalErrors.Try(
            nameof(AddNewTask),
            async () =>
            {
                Func<Task>? remove = null;
                try
                {
                    (string name, object taskCommand, string info) = command;
                    RegisterJobType registration = _registrations.First(d => d.IsCompatible(taskCommand));
                    string jobId = registration.GetId(taskCommand);

                    if(string.IsNullOrWhiteSpace(jobId))
                        return OperationResult.Failure("Die Job Id ist Leer");

                    var entry = new TaskManagerEntry
                                {
                                    Id = Guid.NewGuid().ToString("D"),
                                    ManagerId = registration.Id,
                                    Name = name,
                                    JobId = jobId,
                                    Info = info
                                };

                    IActorRef manager = GetJobManager(registration);

                    await _entrys.InsertOneAsync(entry, token);
                    remove = async () => await _entrys.DeleteOneAsync(_entrys.Operations.Eq(e => e.Id, entry.Id), token);

                    var result = await manager.Ask<IOperationResult>(taskCommand, TimeSpan.FromSeconds(10), token);

                    if(!result.Ok)
                        throw new InvalidOperationException("Task Konnte nicht eingerehit werden");

                    _aggregator.Publish(TasksChanged.Inst);

                    return OperationResult.Success();
                }
                catch (Exception e)
                {
                    if(remove != null)
                        await remove();

                    return OperationResult.Failure(e);
                }
            },
            token,
            () => ImmutableList<ErrorProperty>.Empty
               .Add(new ErrorProperty("Task Name", command.Name))
               .Add(new ErrorProperty("Task Info", command.Info)));

    public async ValueTask<IOperationResult> DeleteTask(string id, CancellationToken token)
        => await _criticalErrors.Try(
            nameof(DeleteTask),
            async () =>
            {
                var filter = _entrys.Operations.Eq(e => e.JobId, id);
                DbTaskManagerEntry ele = await _entrys.Find(filter).SingleAsync(token);
                RegisterJobType registration = _registrations.First(r => r.Id == ele.ManagerId);
                IActorRef manager = GetJobManager(registration);
                object cancelCommand = registration.CreateCancel(ele.JobId);
                var cancelResult = await manager.Ask<IOperationResult>(cancelCommand, TimeSpan.FromSeconds(10), token);

                if(!cancelResult.Ok)
                    return OperationResult.Failure("Der Task konnte nicht Beended werden");

                DbOperationResult deleteResult = await _entrys.DeleteOneAsync(filter, token);
                if(deleteResult.IsAcknowledged)
                {
                    _aggregator.Publish(TasksChanged.Inst);

                    return OperationResult.Success();
                }

                throw new InvalidOperationException("Der Task wurde Abbgebrochen aber nicht aus der Datenbank gelöscht worden.");
            },
            token,
            () => ImmutableList<ErrorProperty>.Empty.Add(new ErrorProperty("Task Id", id)));

    public async ValueTask<TaskManagerEntry[]> GetCurrentTasks(CancellationToken token)
        => await _entrys.ExecuteArray(_entrys.Find(_entrys.Operations.Empty), token);
}