using System.Threading.Tasks;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface IGetSource<TData>
    {
        void DataSource(IExtendedDataSource<MutatingContext<TData>> dataSource);
    }

    public interface ICanQuery<TData> : IGetSource<TData>
        where TData : class, IStateEntity
    {
        Task<TData?> Query(IQuery query);
    }
}