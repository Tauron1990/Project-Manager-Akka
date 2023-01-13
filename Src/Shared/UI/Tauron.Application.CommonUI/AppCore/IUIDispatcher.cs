using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.AppCore;

[PublicAPI]
public interface IUIDispatcher
{
    void Post(Action action);
    Task InvokeAsync(Action action);

    IObservable<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action);

    IObservable<TResult> InvokeAsync<TResult>(Func<TResult> action);
    bool CheckAccess();
}