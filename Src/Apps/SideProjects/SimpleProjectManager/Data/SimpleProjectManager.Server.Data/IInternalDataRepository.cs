using Tauron.Akkatecture.Projections;

namespace SimpleProjectManager.Server.Data;

public interface IInternalDataRepository : IProjectionRepository
{
    IDatabaseCollection<TData> Collection<TData>();
}