using Akka.Streams.Dsl;

namespace Tauron.Application.Akka.Redux.Extensions;

public static class MutateCallbackPlugin
{
    internal static void Install<TState>(IReduxStore<TState> store)
        => store.RegisterReducers(Create.On(
            Flow.Create<(TState State, MutateCallback<TState> Action)>()
               .Select(input => input.Action.Mutator(input.State))));
}