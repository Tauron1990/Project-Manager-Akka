using Akka;
using Akka.Streams.Dsl;

namespace Tauron.Application.Akka.Redux;

public sealed record On<TState>(Flow<DispatchedAction<TState>, TState, NotUsed> Mutator, Type ActionType);