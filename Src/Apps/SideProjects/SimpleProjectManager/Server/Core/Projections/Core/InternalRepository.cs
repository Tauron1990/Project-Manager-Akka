using Akkatecture.Aggregates;
using Akkatecture.Core;
using LiquidProjections;
using MongoDB.Driver;
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

    public Task<TProjection?> Get<TProjection, TIdentity>(ProjectionContext context, TIdentity identity)
        where TProjection : class, IProjectorData<TIdentity>
        where TIdentity : IIdentity
    {
        var db = _database.GetCollection<TProjection>(typeof(TProjection).Name);

        return db.Find(Builders<TProjection>.Filter.Eq(p => p.Id, identity)).FirstOrDefaultAsync()!;
    }

    public Task<TProjection> Create<TProjection, TIdentity>(ProjectionContext context, TIdentity identity, Func<TProjection, bool> shouldoverwite) 
        where TProjection : class, IProjectorData<TIdentity> 
        where TIdentity : IIdentity
        => throw new NotImplementedException();

    public Task<bool> Delete<TIdentity>(ProjectionContext context, TIdentity identity) 
        where TIdentity : IIdentity
        => throw new NotImplementedException();

    public Task Commit<TIdentity>(ProjectionContext context, TIdentity identity) 
        where TIdentity : IIdentity
        => throw new NotImplementedException();

    public long GetLastCheckpoint<TProjection, TIdentity>() where TProjection : class, IProjectorData<TIdentity> 
        where TIdentity : IIdentity
        => throw new NotImplementedException();
}