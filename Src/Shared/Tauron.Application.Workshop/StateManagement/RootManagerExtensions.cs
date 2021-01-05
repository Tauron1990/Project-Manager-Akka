using System;

namespace Tauron.Application.Workshop.StateManagement
{
    public static class RootManagerExtensions
    {
        public static IDisposable ToActionInvoker<TCommand>(this IObservable<TCommand?> commandProvider, IActionInvoker invoker)
            where TCommand : IStateAction
            => commandProvider.NotNull().Subscribe(c => invoker.Run(c));
    }
}