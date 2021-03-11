using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.Workshop.StateManagement.StatePooling
{
    public interface IInitState<TData>
    {
        void Init(ExtendedMutatingEngine<MutatingContext<TData>> engine);
    }

    public interface IPostInit
    {
        void Init(IActionInvoker invoker);
    }
}