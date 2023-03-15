using System.Globalization;
using Akka.Actor;
using Tauron.Operations;

namespace Tauron.Localization.Actor;

public abstract class LocStoreActorBase : UntypedActor
{
    protected sealed override void OnReceive(object message)
    {
        if(message is QueryRequest(var key, var id, var cultureInfo))
            Context.Sender.Tell(new QueryResponse(TryQuery(key, cultureInfo), id));
        else
            base.Unhandled(message);
    }

    protected abstract TriOption<object> TryQuery(string name, CultureInfo target);

    public sealed record QueryRequest(string Key, string Id, CultureInfo CultureInfo);

    public sealed record QueryResponse(TriOption<object> Value, string Id);
}