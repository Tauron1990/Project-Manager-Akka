using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement;

public interface IGetSource<TData>
{
    void DataSource(IExtendedDataSource<MutatingContext<TData>> dataSource);
}