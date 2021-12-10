using System;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public static class RootManagerExtensions
{
    public static IDisposable ToActionInvoker<TCommand>(
        this IObservable<TCommand?> commandProvider,
        IActionInvoker invoker)
        where TCommand : IStateAction
        => commandProvider.NotNull().SubscribeWithStatus(c => invoker.Run(c));
}