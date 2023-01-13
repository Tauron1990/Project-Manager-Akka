using System.Globalization;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Localization.Provider;

namespace Tauron.Localization.Actor;

[PublicAPI]
public sealed class LocCoordinator : ReceiveActor, IWithTimers
{
    private readonly Dictionary<string, Request> _requests = new(StringComparer.Ordinal);

    public LocCoordinator(IEnumerable<ILocStoreProducer> producers)
    {
        producers.Foreach(sp => Context.ActorOf(sp.GetProps(), sp.Name));
        Receive<RequestLocValue>(RequestLocValueHandler);
        Receive<LocStoreActorBase.QueryResponse>(QueryResponseHandler);
        Receive<SendInvalidate>(Invalidate);
    }

    public ITimerScheduler Timers { get; set; } = null!;

    private void QueryResponseHandler(LocStoreActorBase.QueryResponse obj)
    {
        if(!_requests.Remove(obj.Id, out Request? request)) return;

        request.Sender.Tell(new ResponseLocValue(obj.Value, request.Key));
    }

    private void RequestLocValueHandler(RequestLocValue msg)
    {
        (string key, CultureInfo cultureInfo) = msg;
        var request = new Request(Context.Sender, key);
        var opId = Guid.NewGuid().ToString();

        _requests[opId] = request;

        foreach (IActorRef? actorRef in Context.GetChildren())
            actorRef.Tell(new LocStoreActorBase.QueryRequest(request.Key, opId, cultureInfo));

        Timers.StartSingleTimer(Guid.NewGuid(), new SendInvalidate(opId), TimeSpan.FromSeconds(10));
    }

    private void Invalidate(SendInvalidate op)
    {
        if(!_requests.Remove(op.OpId, out Request? request)) return;

        request.Sender.Tell(new ResponseLocValue(Option<object>.None, request.Key));
    }

    private sealed record SendInvalidate(string OpId);

    public sealed record RequestLocValue(string Key, CultureInfo Lang);

    public sealed record ResponseLocValue(Option<object> Result, string Key);

    private sealed record Request(IActorRef Sender, string Key);
}