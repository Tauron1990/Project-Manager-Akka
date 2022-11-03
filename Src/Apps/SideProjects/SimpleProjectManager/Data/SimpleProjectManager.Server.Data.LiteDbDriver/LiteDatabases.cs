using AutoMapper;
using LiteDB;
using SimpleProjectManager.Server.Data.Data;

namespace SimpleProjectManager.Server.Data.LiteDbDriver;

public sealed class LiteDatabases : IDatabases
{
    public IMapper Mapper { get; }
    public IDatabaseCollection<DbFileInfoData> FileInfos { get; }
    public IDatabaseCollection<DbCriticalErrorEntry> CriticalErrors { get; }
    public IDatabaseCollection<DbProjectProjection> ProjectProjections { get; }
    public IDatabaseCollection<DbTaskManagerEntry> TaskManagerEntrys { get; }

    public LiteDatabases(IServiceProvider serviceProvider, ILiteDatabase liteDatabase)
    {
        Mapper = MapperFactory.CreateMapper(serviceProvider);

        FileInfos = new LiteDatabaseCollection<DbFileInfoData>(liteDatabase.GetCollection<DbFileInfoData>());
        CriticalErrors = new LiteDatabaseCollection<DbCriticalErrorEntry>(liteDatabase.GetCollection<DbCriticalErrorEntry>());
        ProjectProjections = new LiteDatabaseCollection<DbProjectProjection>(liteDatabase.GetCollection<DbProjectProjection>());
        TaskManagerEntrys = new LiteDatabaseCollection<DbTaskManagerEntry>(liteDatabase.GetCollection<DbTaskManagerEntry>());
    }
}