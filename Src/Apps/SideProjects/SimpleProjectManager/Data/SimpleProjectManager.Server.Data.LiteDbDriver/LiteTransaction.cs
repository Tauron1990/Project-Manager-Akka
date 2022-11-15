using System.Collections.Concurrent;
using System.Collections.Immutable;
using LiteDB;
using Microsoft.Extensions.Logging;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;
using Tauron;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed partial class LiteTransaction : IDisposable
{
    private readonly ConcurrentDictionary<Type, object> _collections = new();
    private readonly ILiteDatabase _liteDatabase;
    private readonly ILogger<LiteTransaction> _logger;

    private BlockingCollection<Func<Exception?, Exception?>> _actions = new();
    private Exception? _error;

    [LoggerMessage(66, LogLevel.Critical, "Transaction is already Running (Orphan Transaction)")]
    private partial void CriticalTransactionStart(Exception error);
    
    public LiteTransaction(ILiteDatabase liteDatabase, ICriticalErrorService errorService, ILogger<LiteTransaction> logger)
    {
        _liteDatabase = liteDatabase;
        _logger = logger;

        #pragma warning disable EPC13
        Run(
            d =>
            {
                try
                {
                    if(d.BeginTrans())
                        return;

                    d.Rollback();
                    d.BeginTrans();
                }
                catch (Exception e)
                {
                    CriticalTransactionStart(e);
                    errorService.WriteError(new CriticalError(
                        ErrorId.New, 
                        DateTime.Now, 
                        PropertyValue.From(nameof(LiteTransaction)), 
                        SimpleMessage.From("Transaction is already Running (Orphan Transaction)"), 
                        StackTraceData.FromException(e),
                        ImmutableList<ErrorProperty>.Empty), default)
                       .Ignore();
                }
            }).Ignore();
        

        Task.Run(Runner).Ignore();
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