using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Data;

public interface IInternalDataRepository : IProjectionRepository
{
    IDatabases Databases { get; }

    //IDatabaseCollection<TData> Collection<TData>();
}