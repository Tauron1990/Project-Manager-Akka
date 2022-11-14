using Stl.Fusion;
using Tauron.Application.Blazor.Parameters;

namespace Tauron.Application.Blazor;

public static class BlazorViewModelExtensions
{
    public static IState<TValue> GetParameter<TValue>(this IParameterUpdateable updateable, string name, IStateFactory stateFactory)
        => updateable.Updater.Register<TValue>(name, stateFactory);
}