using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public interface IEventAggregator
{
    TEventType GetEvent<TEventType, TPayload>() where TEventType : AggregateEvent<TPayload>, new();
}