using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement;

public interface ICanQuery<TData> : IGetSource<TData>
    where TData : class, IStateEntity
{
    Task<TData?> Query(IQuery query);
}