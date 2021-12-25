using System;
using System.Reactive;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public static class InvokerExtensions
{
    public static IObservable<Unit> ExecuteCommands<TCommand>(
        this IObservable<TCommand?> observable,
        IActionInvoker invoker)
        where TCommand : IStateAction
    {
        return observable
           .Where(c => c != null)
           .Select(
                c =>
                {
                    invoker.Run(c!);

                    return Unit.Default;
                });
    }
}