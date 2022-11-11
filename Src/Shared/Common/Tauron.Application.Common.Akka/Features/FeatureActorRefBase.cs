using Akka.Actor;
using JetBrains.Annotations;

namespace Tauron.Features;

[PublicAPI]
public abstract class FeatureActorRefBase<TInterface> : IFeatureActorRef<TInterface>
    where TInterface : IFeatureActorRef<TInterface>
{
    private readonly TaskCompletionSource<IActorRef> _actorSource = new();

    public Task<IActorRef> Actor => _actorSource.Task;

    async void IFeatureActorRef<TInterface>.Init(Func<Props, Task<IActorRef>> factoryTask, Func<Props> resolver)
    {
        if(_actorSource.Task.IsCompleted)
            throw new InvalidOperationException("Initialization of Actor Compleded");

        try
        {
            var task = factoryTask(resolver());
            if(await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(20))).ConfigureAwait(false) == task)
                _actorSource.TrySetResult(await task.ConfigureAwait(false));
            else
                _actorSource.TrySetCanceled();
        }
        catch (Exception e)
        {
            _actorSource.TrySetException(e);
        }
    }

    public TInterface Tell(object msg)
    {
        OnActor(a => a.Tell(msg));

        return (TInterface)(object)this;
    }

    public TInterface Forward(object msg)
    {
        OnActor(a => a.Forward(msg));

        return (TInterface)(object)this;
    }

    public async Task<TResult> Ask<TResult>(object msg, TimeSpan? timeout = null)
    {
        IActorRef actor = await Actor.ConfigureAwait(false);

        return await actor.Ask<TResult>(msg, timeout).ConfigureAwait(false);
    }

    public void Tell(object message, IActorRef sender)
        => OnActor(a => a.Tell(message, sender));

    private void OnActor(Action<IActorRef> runner)
    {
        if(Actor.IsCompletedSuccessfully)
            runner(Actor.Result);
        else if(Actor.IsCanceled)
            throw new TaskCanceledException(Actor);
        else if(Actor.IsFaulted)
            throw Actor.Exception ?? throw new InvalidOperationException("Unkown Error on executing ActorTask");
        else
            Actor.ContinueWith(
                t =>
                {
                    if(t.IsCompletedSuccessfully)
                        runner(t.Result);
                });
    }
}