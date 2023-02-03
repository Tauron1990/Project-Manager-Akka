using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;

namespace Tauron.Application.Wpf.Dialogs;

public static class DialogExtensions
{
    public static Task<TResult> MakeTask<TResult>(this FrameworkElement ele, Func<TaskCompletionSource<TResult>, object> factory)
    {
        var source = new TaskCompletionSource<TResult>();

        ele.DataContext = factory(source);
        if(ele.DataContext is IDisposable disposable)
            ele.Unloaded += (_, _) => disposable.Dispose();

        return source.Task;
    }

    public static Task<TResult> MakeObsTask<TResult>(this FrameworkElement ele, Func<IObserver<TResult>, object> factory)
    {
        return Observable.Create<TResult>(
                o =>
                {
                    object context = factory(o);
                    ele.DataContext = context;

                    if(context is IDisposable disposable)
                        return disposable;

                    return Disposable.Empty;
                })
            .Take(1).ToTask();
    }
}