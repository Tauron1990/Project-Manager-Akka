using Tauron.Application.AkkaNode.Services.CleanOld;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    [State]
    public sealed class CleanUpManager : StateBase<ToDeleteRevision>, IState<CleanUpTime>
    {
        private readonly IActionInvoker _invoker;

        public CleanUpManager(ExtendedMutatingEngine<MutatingContext<ToDeleteRevision>> engine, IActionInvoker invoker) 
            : base(engine)
            => _invoker = invoker;
    }
}