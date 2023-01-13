namespace Tauron.Applicarion.Redux.Extensions;

public static class MutateCallbackPlugin
{
    internal static void Install<TState>(IReduxStore<TState> store)
        => store.RegisterReducers(Create.On<MutateCallback<TState>, TState>((state, action) => action.Mutator(state)));
}