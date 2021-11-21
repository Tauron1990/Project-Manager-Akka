using System.Collections.Concurrent;
using System.Collections.Immutable;
using Akka.Actor;
using Akka.Actor.Dsl;
using Akkatecture.Jobs;
using Akkatecture.Jobs.Commands;
using MongoDB.Bson;
using MongoDB.Driver;
using ReactiveUI;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Shared.Services;
using Tauron.Operations;

#pragma warning disable EX006

namespace SimpleProjectManager.Server.Core.Tasks;

public sealed record TaskManagerEntry(ObjectId Id, string ManagerId, string Name, string JobId, string Info);

public sealed class TaskManagerCore
{
    private readonly IMongoCollection<TaskManagerEntry> _entrys;
    private readonly ConcurrentBag<RegisterJobType> _registrations = new();
    private readonly ConcurrentDictionary<string, IActorRef> _jobManagers = new();
    private readonly ActorSystem _actorSystem;
    private readonly CriticalErrorHelper _criticalErrors;

    private readonly ConcurrentDictionary<string, TaskManagerJobId> _autoCancelDely = new();
    private IActorRef _autoRemoveManager = Nobody.Instance;

    public TaskManagerCore(InternalDataRepository repository, ActorSystem actorSystem, ICriticalErrorService criticalErrorService, ILogger<TaskManagerCore> logger)
    {
        _actorSystem = actorSystem;
        _criticalErrors = new CriticalErrorHelper(nameof(TaskManagerCore), criticalErrorService, logger);
        _entrys = repository.Collection<TaskManagerEntry>();
        
        InitAutoDelete();
    }

    private void InitAutoDelete()
    {
        _autoRemoveManager = _actorSystem.ActorOf(Props.Create(() => new TaskManagerDeleteEntryManager(_entrys, _criticalErrors)));
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
                if (_autoCancelDely.TryRemove(evt.GetJobId(), out var cancelId)) 
                    _autoRemoveManager.Tell(new Cancel<TaskManagerDeleteEntry, TaskManagerJobId>(cancelId));
                break;
        }
    }

    private void ProcessFinish(ISchedulerEvent evt)
    {
        if (evt is not ISchedulerEvent<TaskManagerDeleteEntry, TaskManagerJobId>)
        {
            _autoCancelDely.AddOrUpdate(
                evt.GetJobId(),
                jobId =>
                {
                    var id = TaskManagerJobId.New;
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

            if (string.IsNullOrWhiteSpace(toRemove.Key)) return;

            _autoCancelDely.TryRemove(toRemove.Key, out _);
        }
    }

    private IActorRef GetJobManager(RegisterJobType registration)
        => _jobManagers.GetOrAdd(registration.Id,
            static (_, fac) => fac.System.ActorOf(fac.JobManagerFactory()), 
            (registration.JobManagerFactory, System:_actorSystem));

    public void Register(RegisterJobType registerJobType)
        => _registrations.Add(registerJobType);

    public async ValueTask<IOperationResult> AddNewTask(AddTaskCommand command, CancellationToken token)
        => await _criticalErrors.Try(
            nameof(AddNewTask),
            async () =>
            {
                Func<Task>? remove = null;
                try
                {
                    var (name, taskCommand, info) = command;
                    var registration = _registrations.First(d => d.IsCompatible(taskCommand));
                    var jobId = registration.GetId(taskCommand);

                    if (string.IsNullOrWhiteSpace(jobId))
                        return OperationResult.Failure("Die Job Id ist Leer");

                    var entry = new TaskManagerEntry(ObjectId.GenerateNewId(), registration.Id, name, jobId, info);

                    var manager = GetJobManager(registration);
                    
                    await _entrys.InsertOneAsync(entry, cancellationToken: token);
                    remove = async () => await _entrys.DeleteOneAsync(Builders<TaskManagerEntry>.Filter.Eq(e => e.Id, entry.Id), token);
                    
                    var result = await manager.Ask<IOperationResult>(taskCommand, TimeSpan.FromSeconds(10), token);

                    if (!result.Ok)
                        throw new InvalidOperationException("Task Konnte nicht eingerehit werden");

                    MessageBus.Current.SendMessage(TasksChanged.Inst);
                    return OperationResult.Success();
                }
                catch (Exception e)
                {
                    if (remove != null)
                        await remove();

                    return OperationResult.Failure(e);
                }
            },
            token,
            () => ImmutableList<ErrorProperty>.Empty
               .Add(new ErrorProperty("Task Name", command.Name))
               .Add(new ErrorProperty("Task Info", command.Info)));

    public async ValueTask<IOperationResult> Delete(string id, CancellationToken token)
        => await _criticalErrors.Try(
            nameof(Delete),
            async () =>
            {
                var filter = Builders<TaskManagerEntry>.Filter.Eq(e => e.JobId, id);
                var ele = await _entrys.Find(filter).SingleAsync(token);
                var registration = _registrations.First(r => r.Id == ele.ManagerId);
                var manager = GetJobManager(registration);
                var cancelCommand = registration.CreateCancel(ele.JobId);
                var cancelResult = await manager.Ask<IOperationResult>(cancelCommand, TimeSpan.FromSeconds(10), token);
                if(!cancelResult.Ok)
                    return OperationResult.Failure("Der Task konnte nicht Beended werden");

                var deleteResult = await _entrys.DeleteOneAsync(filter, token);
                if (deleteResult.IsAcknowledged)
                {
                    MessageBus.Current.SendMessage(TasksChanged.Inst);
                    return OperationResult.Success();
                }
                
                throw new InvalidOperationException("Der Task wurde Abbgebrochen aber nicht aus der Datenbank gelöscht worden.");
            },
            token,
            () => ImmutableList<ErrorProperty>.Empty.Add(new ErrorProperty("Task Id", id)));

    public async ValueTask<TaskManagerEntry[]> GetCurrentTasks(CancellationToken token)
        => (await _entrys.Find(Builders<TaskManagerEntry>.Filter.Empty).ToListAsync(token)).ToArray();
}