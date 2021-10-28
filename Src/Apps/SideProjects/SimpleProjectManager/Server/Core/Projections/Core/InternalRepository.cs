using Akkatecture.Core;
using LiquidProjections;
using MongoDB.Driver;
using Tauron;
using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Core.Projections.Core;

public sealed class InternalRepository : IProjectionRepository
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<CheckPointInfo> _checkpointInfo;

    public InternalRepository(IMongoDatabase database)
    {
        _database = database;
        _checkpointInfo = database.GetCollection<CheckPointInfo>(nameof(CheckPointInfo));
    }

    private static string GetDatabaseId(Type type)
        => type.FullName ?? type.Name;

    private async Task<IMongoCollection<TData>> Collection<TData>()
    {
        // ReSharper disable once InvertIf
        if (context is not null)
        {
            var id = GetDatabaseId(typeof(TData));
            var data = new CheckPointInfo { Checkpoint = context.Checkpoint, Id = id };
            var filter = Builders<CheckPointInfo>.Filter.Eq(cp => cp.Id, id);
            var option = new ReplaceOptions { IsUpsert = true };

            await _checkpointInfo.ReplaceOneAsync(filter, data, option);
        }

        return _database.GetCollection<TData>(typeof(TData).Name);
    }

    public async Task<TProjection?> Get<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
        => await (await Collection<TProjection>(context)).Find(Builders<TProjection>.Filter.Eq(p => p.Id, identity)).FirstOrDefaultAsync()!;

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

        var coll = await Collection<TProjection>(context);
        var filter = Builders<TProjection>.Filter.Eq(p => p.Id, identity);

        var data = await coll.Find(filter).FirstOrDefaultAsync();
        if (data == null)
        {
            data = DataFactory();
            await coll.InsertOneAsync(data);
        }
        else if (shouldoverwite(data))
        {
            data = DataFactory();
            await coll.ReplaceOneAsync(filter, data);
        }

        return data;
    }

    public async Task<bool> Delete<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {
        var coll = await Collection<TProjection>(context);
        var filter = Builders<TProjection>.Filter.Eq(p => p.Id, identity);
        var result = await coll.DeleteOneAsync(filter);

        return result.DeletedCount == 1;
    }

    public async Task Commit<TProjection, TIdentity>(ProjectionContext context, TProjection projection, TIdentity identity) 
        where TIdentity : IIdentity
        where TProjection : class, IProjectorData<TIdentity>
    {

    }

    public long GetLastCheckpoint<TProjection, TIdentity>() where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
    {
        var filter = Builders<CheckPointInfo>.Filter.Eq(p => p.Id, GetDatabaseId(typeof(TProjection)));

        return _checkpointInfo.Find(filter).FirstOrDefault()?.Checkpoint ?? 0;
    }
}