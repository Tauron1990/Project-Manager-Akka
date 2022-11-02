using AutoMapper;
using SimpleProjectManager.Server.Data.Data;

namespace SimpleProjectManager.Server.Data;

public interface IDatabases
{
    IMapper Mapper { get; }

    public IDatabaseCollection<DbFileInfoData> FileInfos { get; }
    
    public IDatabaseCollection<DbCriticalErrorEntry> CriticalErrors { get; }
}