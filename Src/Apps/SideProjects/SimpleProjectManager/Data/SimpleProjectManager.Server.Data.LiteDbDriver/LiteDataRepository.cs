using System.Collections.Concurrent;
using Akkatecture.Core;
using LiquidProjections;
using LiteDB;
using LiteDB.Async;
using Tauron;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteDataRepository : IInternalDataRepository
{
    private readonly ILiteDatabaseAsync _database;
    private readonly ConcurrentDictionary<object, Task<ILiteDatabaseAsync>> _transactions = new();

    public LiteDataRepository(ILiteDatabaseAsync database)
    {
        _database = database;
        SerializationHelper<CheckPointInfo>.Register();
    }

    private Task<ILiteDatabaseAsync> GetTransaction(object key)
        => _transactions.GetOrAdd(key, static (_, db) => db.BeginTransactionAsync(), _database);

    private static string GetDatabaseId(Type type)
        => type.FullName ?? type.Name;

    private async Task CommitCheckpoint<TData>(ILiteDatabaseAsync database, ProjectionContext context, object key)
    {
        var id = GetDatabaseId(typeof(TData));
        var data = new CheckPointInfo { Checkpoint = context.Checkpoint, Id = id };
        await database.GetCollection<CheckPointInfo>().UpsertAsync(data);
        await database.CommitAsync();
        _transactions.TryRemove(key, out _);
    }

    public IDatabaseCollection<TData> Collection<TData>()
        => new LiteDatabaseCollection<TData>(InternalCollection<TData>());
    
    private ILiteCollectionAsync<TData> InternalCollection<TData>(ILiteDatabaseAsync? transaction = null)
    {
        SerializationHelper<TData>.Register();
        return (transaction ?? _database).GetCollection<TData>(typeof(TData).Name);
    }

    public async Task<TProjection?> Get<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
        => await InternalCollection<TProjection>().FindByIdAsync(new BsonValue(identity));

    public async Task<TProjection> Create<TProjection, TIdentity>(ProjectionContext context, TIdentity identity, Func<TProjection, bool> shouldoverwite) 
        where TProjection : class, IProjectorData<TIdentity> 
        where TIdentity : IIdentity
    {
        TProjection DataFactory()
        {
            if (FastReflection.Shared.FastCreateInstance(typeof(TProjection)) is not TProjection data)
                throw new InvalidOperationException("Projection Creation Failed");

            return data;
        }

        var coll = InternalCollection<TProjection>();
        
        var data = await coll.FindByIdAsync(new BsonValue(identity));
        if (data == null)
            data = DataFactory();
        else if (shouldoverwite(data))
        {
            data = DataFactory();
            await coll.UpdateAsync(data);
        }

        return data;
    }

    public async Task<bool> Delete<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {
        using var transaction = await GetTransaction(identity);
        try
        {
            var coll = InternalCollection<TProjection>(transaction);
            var result = await coll.DeleteAsync(new BsonValue(identity));
            await CommitCheckpoint<TProjection>(transaction, context, identity);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }

    public async Task Commit<TProjection, TIdentity>(ProjectionContext context, TProjection projection, TIdentity identity) 
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {
        var trans = await GetTransaction(identity);
        try
        {
            var coll = InternalCollection<TProjection>(trans);

            await coll.UpsertAsync(projection);
            await CommitCheckpoint<TProjection>(trans, context, identity);
        }
        catch
        {
            await trans.RollbackAsync();

            throw;
        }
    }

    public async Task Completed<TIdentity>(TIdentity identity)
        where TIdentity : IIdentity
    {
        if (!_transactions.TryRemove(identity, out var transaction)) return;

        await (await transaction).RollbackAsync();
        transaction.Dispose();
    }

    public long GetLastCheckpoint<TProjection, TIdentity>() where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
    {
        return _database.UnderlyingDatabase.GetCollection<CheckPointInfo>()
           .FindById(GetDatabaseId(typeof(TProjection)))?.Checkpoint ?? 0;
    }
}