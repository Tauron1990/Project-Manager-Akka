using System.Globalization;
using Akka.Actor;
using Akka.Util;

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

    protected abstract Option<object> TryQuery(string name, CultureInfo target);

    public sealed record QueryRequest(string Key, string Id, CultureInfo CultureInfo);

    public sealed record QueryResponse(Option<object> Value, string Id);
}