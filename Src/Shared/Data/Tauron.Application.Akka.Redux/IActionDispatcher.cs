using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;

namespace Tauron.Application.Akka.Redux;

public interface IActionDispatcher
{
    bool CanProcess<TAction>();

    bool CanProcess(Type type);

    Source<TAction, NotUsed> ObservAction<TAction>()
        where TAction : class;

    Task<IQueueOfferResult> Dispatch(object action);

    Sink<object, NotUsed> Dispatcher();
}