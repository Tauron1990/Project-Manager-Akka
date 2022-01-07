using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Tauron.Applicarion.Redux;

namespace TestApp;

public sealed record TestState(int Counter);

public sealed record IncremntAction;

public sealed record IncrementMiddlewareAction;

public sealed class TestMiddleware : Middleware<TestState>
{
    public TestMiddleware()
    {
        OnAction<IncrementMiddlewareAction>(observable => from typed in observable
                                                          select typed.NewAction(new IncremntAction()));
    }
}

static class Program
{
    static void Main()
    {
        using var store = Create.Store(new TestState(0), Scheduler.CurrentThread);

        store.RegisterMiddlewares(new TestMiddleware());
        store.RegisterReducers(Create.On<IncremntAction, TestState>(s => s with{ Counter = s.Counter + 1}));
        store.RegisterEffects(Create.Effect<TestState>(s => s.Select().Where(ts => ts.Counter == 2).Select(_ => new IncremntAction())));
        
        using var logger = store.Select().Subscribe(s => Console.WriteLine(s.ToString()));
        
        store.Dispatch(new IncremntAction());
        store.Dispatch(new IncrementMiddlewareAction());
    }
}