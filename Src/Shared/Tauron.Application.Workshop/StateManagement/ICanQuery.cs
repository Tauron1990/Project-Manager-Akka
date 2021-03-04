using System.Threading.Tasks;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement
{
    public interface ICanQuery<TData>
        where TData : class, IStateEntity
    {
        Task<TData?> Query(IQuery query);

        void DataSource(IExtendedDataSource<MutatingContext<TData>> dataSource);
    }
}