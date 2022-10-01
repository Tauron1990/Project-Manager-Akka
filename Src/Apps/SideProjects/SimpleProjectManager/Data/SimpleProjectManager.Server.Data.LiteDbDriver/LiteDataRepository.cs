using System.Collections.Concurrent;
using Akkatecture.Core;
using LiquidProjections;
using LiteDB;
using SimpleProjectManager.Shared;
using Tauron;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteDataRepository : IInternalDataRepository
{
    private readonly ILiteDatabase _database;
    private readonly ConcurrentDictionary<object, LiteTransaction> _transactions = new();

    public LiteDataRepository(ILiteDatabase database)
    {
        _database = database;
        SerializationHelper<CheckPointInfo>.Register();
    }

    private LiteTransaction GetTransaction(object key)
        => _transactions.GetOrAdd(key, static (_, db) => new LiteTransaction(db), _database);

    private static string GetDatabaseId(Type type)
        => type.FullName ?? type.Name;

    private async Task CommitCheckpoint<TData>(ProjectionContext context, object key)
    {
        if(!_transactions.TryRemove(key, out var trans))
            throw new InvalidOperationException($"No Transaction with {key} Found");

        using (trans)
        {
            await trans.Run(
                d =>
                {
                    var id = GetDatabaseId(typeof(TData));
                    var data = new CheckPointInfo { Checkpoint = context.Checkpoint, Id = id };
                    d.GetCollection<CheckPointInfo>().Upsert(data);
                });
            await trans.Run(d => d.Commit());
        }
    }

    public IDatabaseCollection<TData> Collection<TData>()
        => new LiteDatabaseCollection<TData>(InternalCollection<TData>());
    
    private ILiteCollection<TData> InternalCollection<TData>(LiteTransaction? transaction = null)
    {
        SerializationHelper<TData>.Register();
        
        return transaction is null ? _database.GetCollection<TData>() : transaction.Collection<TData>();
    }

    public Task<TProjection?> Get<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
        => To.Task(() => InternalCollection<TProjection>().FindById(identity.Value))!;

    public Task<TProjection> Create<TProjection, TIdentity>(ProjectionContext context, TIdentity identity, Func<TProjection, bool> shouldoverwite) 
        where TProjection : class, IProjectorData<TIdentity> 
        where TIdentity : IIdentity
    {
        TProjection DataFactory()
        {
            if (FastReflection.Shared.FastCreateInstance(typeof(TProjection)) is not TProjection data)
                throw new InvalidOperationException("Projection Creation Failed");

            data.Id = identity;
            
            return data;
        }

        return To.Task(
            () =>
            {
                var coll = InternalCollection<TProjection>();
                
                var data = coll.FindById(identity.Value);
                if(data == null)
                    data = DataFactory();
                else if(shouldoverwite(data))
                {
                    coll.Delete(identity.Value);
                    data = DataFactory();
                }

                return data;
            });
    }

    public async Task<bool> Delete<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {
        using var transaction = GetTransaction(identity);
        
        var coll = InternalCollection<TProjection>(transaction);
        var result = await transaction.Run(() => coll.Delete(identity.Value));
        await CommitCheckpoint<TProjection>(context, identity);

        return result;
    }

    public async Task Commit<TProjection, TIdentity>(ProjectionContext context, TProjection projection, TIdentity identity)
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {
        using var trans = GetTransaction(identity);
        var coll = InternalCollection<TProjection>(trans);

        await trans.Run(() => coll.Upsert(identity.Value, projection));
        await CommitCheckpoint<TProjection>(context, identity);
    }

    public Task Completed<TIdentity>(TIdentity identity)
        where TIdentity : IIdentity
    {
        if (!_transactions.TryRemove(identity, out var transaction)) return Task.CompletedTask;

        transaction.Dispose();
        
        return Task.CompletedTask;
    }

    public long? GetLastCheckpoint<TProjection, TIdentity>() where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
    {
        return _database.GetCollection<CheckPointInfo>()
           .FindById(GetDatabaseId(typeof(TProjection)))?.Checkpoint;
    }
}