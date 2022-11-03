using AutoMapper;
using MongoDB.Driver;
using SimpleProjectManager.Server.Data.Data;

namespace SimpleProjectManager.Server.Data.MongoDb;

public sealed class MongoDataBases : IDatabases
{
    public IMapper Mapper { get; }
    
    public IDatabaseCollection<DbFileInfoData> FileInfos { get; }
    public IDatabaseCollection<DbCriticalErrorEntry> CriticalErrors { get; }
    public IDatabaseCollection<DbProjectProjection> ProjectProjections { get; }
    public IDatabaseCollection<DbTaskManagerEntry> TaskManagerEntrys { get; }

    public MongoDataBases(IServiceProvider serviceProvider, IMongoDatabase mongoDatabase)
    {
        Mapper = MapperFactory.CreateMapper(serviceProvider);

        FileInfos = new DatabaseCollection<DbFileInfoData>(mongoDatabase.GetCollection<DbFileInfoData>(nameof(DbFileInfoData)));
        CriticalErrors = new DatabaseCollection<DbCriticalErrorEntry>(mongoDatabase.GetCollection<DbCriticalErrorEntry>(nameof(DbCriticalErrorEntry)));
        ProjectProjections = new DatabaseCollection<DbProjectProjection>(mongoDatabase.GetCollection<DbProjectProjection>(nameof(DbProjectProjection)));
        TaskManagerEntrys = new DatabaseCollection<DbTaskManagerEntry>(mongoDatabase.GetCollection<DbTaskManagerEntry>(nameof(DbTaskManagerEntry)));
    }
}