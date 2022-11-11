using Akka;
using Akka.Streams.Dsl;
using JetBrains.Annotations;

namespace Tauron.Application.Akka.Redux.Configuration;

[PublicAPI]
public interface IEffectFactory<TState>
    where TState : class, new()
{
    IEffect<TState> CreateEffect(Func<Source<object, NotUsed>> run);

    IEffect<TState> CreateEffect(Func<Source<TState, NotUsed>, Source<object, NotUsed>> run);

    IEffect<TState> CreateEffect<TAction>(Func<Source<(TAction Action, TState State), NotUsed>, Source<object, NotUsed>> run)
        where TAction : class;
}