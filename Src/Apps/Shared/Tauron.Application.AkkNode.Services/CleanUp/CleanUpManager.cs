using Tauron.Application.AkkaNode.Services.CleanUp.Core;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.StatePooling;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    [State]
    [DefaultDispatcher]
    public sealed class CleanUpManager : IState<ToDeleteRevision>, IState<CleanUpTime>, IPooledState
    {
    }
}