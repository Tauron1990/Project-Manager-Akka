using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.StatePooling;

namespace Tauron.Application.Workshop.StateManagement.Internal;

public sealed class PhysicalInstance : IStateInstance
{
    private bool _initCalled;

    public PhysicalInstance(object state) => ActualState = state;

    public object ActualState { get; }

    public void InitState<TData>(ExtendedMutatingEngine<MutatingContext<TData>> engine)
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        if(ActualState is IInitState<TData> init)
            init.Init(engine);
    }

    public void ApplyQuery<TData>(IExtendedDataSource<MutatingContext<TData>> engine) where TData : class, IStateEntity
    {
        if(ActualState is IGetSource<TData> canQuery)
            canQuery.DataSource(engine);
    }

    public void PostInit(IActionInvoker actionInvoker)
    {
        if(_initCalled) return;

        _initCalled = true;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if(ActualState is IPostInit postInit)
            postInit.Init(actionInvoker);
    }
}