using AutoMapper;
using SimpleProjectManager.Server.Data.Data;

namespace SimpleProjectManager.Server.Data;

public interface IDatabases
{
    IMapper Mapper { get; }

    IDatabaseCollection<DbFileInfoData> FileInfos { get; }

    IDatabaseCollection<DbCriticalErrorEntry> CriticalErrors { get; }

    IDatabaseCollection<DbProjectProjection> ProjectProjections { get; }

    IDatabaseCollection<DbTaskManagerEntry> TaskManagerEntrys { get; }
}