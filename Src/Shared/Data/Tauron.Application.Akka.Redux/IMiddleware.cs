using Akka;
using Akka.Streams.Dsl;

namespace Tauron.Application.Akka.Redux;

public interface IMiddleware
{
    void Init(IRootStore rootStore);

    Source<object, NotUsed> Connect(IRootStore actionObservable);
}