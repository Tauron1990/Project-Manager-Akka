using System.Collections.Concurrent;
using Akkatecture.Core;
using LiquidProjections;
using MongoDB.Driver;
using Tauron;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Data.MongoDb;

public sealed class InternalDataRepository : IInternalDataRepository
{
    private readonly ConcurrentDictionary<object, IClientSessionHandle> _transactions = new();
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<CheckPointInfo> _checkpointInfo;

    public IDatabases Databases { get; }
    
    public InternalDataRepository(IServiceProvider serviceProvider, IMongoDatabase database)
    {
        _database = database;
        _checkpointInfo = database.GetCollection<CheckPointInfo>(nameof(CheckPointInfo));
        Databases = new MongoDataBases(serviceProvider, database);
    }

    private static string GetDatabaseId(Type type)
        => type.FullName ?? type.Name;

    private IClientSessionHandle GetTransaction(object key)
        => _transactions.GetOrAdd(
            key,
            static (_, db) =>
            {
                IClientSessionHandle? session = db.Client.StartSession();

                if(session.IsInTransaction)
                    return session;

                try
                {
                    session.StartTransaction();
                }
                catch (NotSupportedException) { }

                return session;
            },
            _database);

    private async Task CommitCheckpoint<TData>(IClientSessionHandle? handle, ProjectionContext context)
    {
        string id = GetDatabaseId(typeof(TData));
        var data = new CheckPointInfo { Checkpoint = context.Checkpoint, Id = id };
        var filter = Builders<CheckPointInfo>.Filter.Eq(cp => cp.Id, id);
        var option = new ReplaceOptions { IsUpsert = true };

        if(handle == null)
            await _checkpointInfo.ReplaceOneAsync(filter, data, option);
        else
            await _checkpointInfo.ReplaceOneAsync(handle, filter, data, option);
    }

    private IMongoCollection<TData> InternalCollection<TData>()
        => _database.GetCollection<TData>(typeof(TData).Name);

    public async Task<TProjection?> Get<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
        => await InternalCollection<TProjection>().Find(Builders<TProjection>.Filter.Eq(p => p.Id, identity)).FirstOrDefaultAsync()!;

    public async Task<TProjection> Create<TProjection, TIdentity>(ProjectionContext context, TIdentity identity, Func<TProjection, bool> shouldoverwite) 
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

        var coll = InternalCollection<TProjection>();
        var filter = Builders<TProjection>.Filter.Eq(p => p.Id, identity);

        TProjection? data = await coll.Find(filter).FirstOrDefaultAsync();
        if (data == null)
            data = DataFactory();
        else if (shouldoverwite(data))
        {
            data = DataFactory();
            IClientSessionHandle trans = GetTransaction(identity);
            await coll.ReplaceOneAsync(trans, filter, data);
        }

        return data;
    }

    public async Task<bool> Delete<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {
        var coll = InternalCollection<TProjection>();
        var filter = Builders<TProjection>.Filter.Eq(p => p.Id, identity);
        DeleteResult? result = await coll.DeleteOneAsync(filter);
        await CommitCheckpoint<TProjection>(null, context);

        return result.DeletedCount == 1;
    }

    public async Task Commit<TProjection, TIdentity>(ProjectionContext context, TProjection projection, TIdentity identity) 
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {
        IClientSessionHandle trans = GetTransaction(identity);
        var filter = Builders<TProjection>.Filter.Eq(p => p.Id, identity);
        var options = new ReplaceOptions { IsUpsert = true };
        var coll = InternalCollection<TProjection>();

        await coll.ReplaceOneAsync(trans, filter, projection, options);
        await CommitCheckpoint<TProjection>(trans, context);

        if(trans.IsInTransaction)
            await trans.CommitTransactionAsync();
    }

    public async Task Completed<TIdentity>(TIdentity identity)
        where TIdentity : IIdentity
    {
        if (!_transactions.TryRemove(identity, out IClientSessionHandle? transaction)) return;

        if (transaction.IsInTransaction)
            await transaction.AbortTransactionAsync();
        transaction.Dispose();
    }

    public long? GetLastCheckpoint<TProjection, TIdentity>() where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
    {
        var filter = Builders<CheckPointInfo>.Filter.Eq(p => p.Id, GetDatabaseId(typeof(TProjection)));

        return _checkpointInfo.Find(filter).FirstOrDefault()?.Checkpoint ?? 0;
    }
}