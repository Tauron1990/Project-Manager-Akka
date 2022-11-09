using System.Collections.Concurrent;
using LiteDB;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteTransaction : IDisposable
{
    private readonly ConcurrentDictionary<Type, object> _collections = new();
    private readonly ILiteDatabase _liteDatabase;

    private BlockingCollection<Func<Exception?, Exception?>> _actions = new();
    private Exception? _error;

    public LiteTransaction(ILiteDatabase liteDatabase)
    {
        _liteDatabase = liteDatabase;
        Run(
            d =>
            {
                if(d.BeginTrans())
                    return;

                throw new InvalidOperationException("Transaction is already Running (Orphan Transaction)");
            });
        Task.Run(Runner);
    }

    public void Dispose()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if(_actions is null || _actions.IsAddingCompleted) return;

        _actions.CompleteAdding();
    }

    private void Runner()
    {
        foreach (var action in _actions.GetConsumingEnumerable())
        {
            Exception? err = action(_error);

            if(err is null) continue;

            _error = err;
        }

        _liteDatabase.Rollback();
        Interlocked.Exchange(ref _actions, null!).Dispose();
        _collections.Clear();
    }

    private Func<Exception?, Exception?> Warp(TaskCompletionSource task, Action<TaskCompletionSource> action)
    {
        return err =>
               {
                   if(err is not null)
                       task.SetException(err);

                   try
                   {
                       action(task);

                       return null;
                   }
                   catch (Exception e)
                   {
                       Interlocked.Exchange(ref _error, e);
                       task.SetException(e);

                       return e;
                   }
                   finally
                   {
                       task.TrySetCanceled();
                   }
               };
    }

    private Func<Exception?, Exception?> Warp<TResult>(TaskCompletionSource<TResult> task, Action<TaskCompletionSource<TResult>> action)
    {
        return err =>
               {
                   if(err is not null)
                       task.SetException(err);

                   try
                   {
                       action(task);

                       return null;
                   }
                   catch (Exception e)
                   {
                       Interlocked.Exchange(ref _error, e);
                       task.SetException(e);

                       return e;
                   }
                   finally
                   {
                       task.TrySetCanceled();
                   }
               };
    }


    public ILiteCollection<TData> Collection<TData>()
        => (ILiteCollection<TData>)_collections.GetOrAdd(typeof(TData), static (_, db) => db.GetCollection<TData>(), _liteDatabase);

    public Task Run(Action<ILiteDatabase> action)
    {
        if(_error is not null)
            return Task.FromException(_error);

        var task = new TaskCompletionSource();
        _actions.Add(
            Warp(
                task,
                t =>
                {
                    action(_liteDatabase);
                    t.SetResult();
                }));

        return task.Task;
    }

    public Task Run(Action action)
    {
        if(_error is not null)
            return Task.FromException(_error);

        var task = new TaskCompletionSource();
        _actions.Add(
            Warp(
                task,
                t =>
                {
                    action();
                    t.SetResult();
                }));

        return task.Task;
    }

    public Task<TResult> Run<TResult>(Func<TResult> action)
    {
        if(_error is not null)
            return Task.FromException<TResult>(_error);

        var task = new TaskCompletionSource<TResult>();
        _actions.Add(Warp(task, t => t.SetResult(action())));

        return task.Task;
    }
}