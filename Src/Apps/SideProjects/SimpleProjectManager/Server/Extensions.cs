namespace SimpleProjectManager.Server;

public static class Extensions
{
    public static IObservable<TEvent> SubscribeTo<TEvent>(this IEventAggregator aggregator)
        => aggregator.GetEvent<AggregateEvent<TEvent>, TEvent>().Get();

    public static void Publish<TEvent>(this IEventAggregator aggregator, TEvent evt)
        => aggregator.GetEvent<AggregateEvent<TEvent>, TEvent>().Publish(evt);
}