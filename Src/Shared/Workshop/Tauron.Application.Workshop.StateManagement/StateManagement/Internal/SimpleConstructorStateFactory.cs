using Stl;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement.Builder;

namespace Tauron.Application.Workshop.StateManagement.Internal;

public class SimpleConstructorStateFactory : IStateInstanceFactory
{
    public int Order => int.MaxValue;

    public bool CanCreate(Type state)
        => true;

    public Option<IStateInstance> Create<TData>(
        Type state, IServiceProvider? serviceProvider, ExtendedMutatingEngine<MutatingContext<TData>> dataEngine, IActionInvoker invoker)
    {
        return (from constructorInfo in state.GetConstructors()
                let param = constructorInfo.GetParameters()
                select param.Length switch
                {
                    0 => FastReflection.Shared.FastCreateInstance(state),
                    1 => param[0].ParameterType.IsAssignableTo(typeof(IActionInvoker))
                        ? FastReflection.Shared.GetCreator(constructorInfo)(new object?[] { invoker })
                        : FastReflection.Shared.GetCreator(constructorInfo)(new object?[] { dataEngine }),
                    2 => FastReflection.Shared.GetCreator(constructorInfo)(
                        param[0].ParameterType.IsAssignableTo(typeof(IActionInvoker))
                            ? new object?[] { invoker, dataEngine }
                            : new object?[] { dataEngine, invoker }),
                    _ => null,
                }
                into instance
                where instance is not null
                select new PhysicalInstance(instance)).FirstOrDefault();
    }
}