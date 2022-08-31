using System.Collections.Concurrent;
using System.Reactive;
using System.Runtime.ExceptionServices;
using LiteDB;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteTransaction : IDisposable
{
    private readonly record struct RunAction(TaskCompletionSource Task, Action<ILiteDatabase> Action);
    
    private readonly ILiteDatabase _liteDatabase;
    private readonly BlockingCollection<RunAction> _actions = new();
    private Exception? _error;
    
    public LiteTransaction(ILiteDatabase liteDatabase)
    {
        _liteDatabase = liteDatabase;
        Run(d =>
            {
                if(d.BeginTrans())
                    return;

                throw new InvalidOperationException("Transaction is already Running (Orphan Transaction)");
            });
        Task.Run(Runner);
    }

    private void Runner()
    {
        foreach (var action in _actions.GetConsumingEnumerable())
        {
            if(_error is not null)
            {
                action.Task.SetException(_error);
                continue;
            }
            
            try
            {
                action.Action(_liteDatabase);
                action.Task.SetResult();
            }
            catch(Exception e)
            {
                Interlocked.Exchange(ref _error, e);
                action.Task.SetException(e);
            }
        }
        
        _actions.Dispose();
    }

    public Task Run(Action<ILiteDatabase> action)
    {
        if(_error is not null)
            return Task.FromException(_error);

        var task = new TaskCompletionSource();
        _actions.Add(new RunAction(task, action));

        return task.Task;
    }

    public void Dispose()
        => _actions.CompleteAdding();
}