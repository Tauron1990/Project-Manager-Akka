using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public static class RootManagerExtensions
{
    public static IDisposable ToActionInvoker<TCommand>(
        this IObservable<TCommand?> commandProvider,
        IActionInvoker invoker,
        Action<Exception>? errorHandler)
        where TCommand : IStateAction
    {
        void OnNext(TCommand c)
            => invoker.Run(c);

        if(errorHandler is null)
            return commandProvider.NotNull().Subscribe(OnNext);

        return commandProvider.NotNull().Subscribe(OnNext, errorHandler);
    }
}