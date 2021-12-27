using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.TAkka;

[PublicAPI]
public abstract class BaseActorRef<TActor>
    where TActor : ActorBase
{
    private readonly ActorRefFactory<TActor> _builder;

    protected BaseActorRef(ActorRefFactory<TActor> actorBuilder) => _builder = actorBuilder;

    public bool IsInitialized { get; private set; }

    protected virtual bool IsSync => false;

    public IActorRef Actor { get; private set; } = ActorRefs.Nobody;

    public ActorPath Path => Actor.Path;

    public event Action? Initialized;

    public void Tell(object message, IActorRef sender)
    {
        Actor.Tell(message, sender);
    }

    public bool Equals(IActorRef? other) => Actor.Equals(other);

    public int CompareTo(IActorRef? other) => Actor.CompareTo(other);

    public int CompareTo(object? obj) => Actor.CompareTo(obj);

    #pragma warning disable AV1551
    public virtual void Init(string? name = null)
        #pragma warning restore AV1551
    {
        CheckIsInit();
        Actor = IsSync ? _builder.CreateSync(name) : _builder.Create(name);
        IsInitialized = true;
        OnInitialized();
    }

    public virtual void Init(IActorRefFactory factory, string? name = null)
    {
        CheckIsInit();
        Actor = factory.ActorOf(IsSync ? _builder.CreateSyncProps() : _builder.CreateProps(), name);
        IsInitialized = true;
        OnInitialized();
    }

    protected void ResetInternal()
    {
        Actor.Tell(PoisonPill.Instance);
        Actor = ActorRefs.Nobody;
        IsInitialized = false;
    }

    protected void CheckIsInit()
    {
        if (IsInitialized)
            throw new InvalidOperationException("ActorRef is Init");
    }

    protected virtual void OnInitialized()
        => Initialized?.Invoke();
}