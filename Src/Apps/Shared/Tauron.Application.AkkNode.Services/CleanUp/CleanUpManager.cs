using JetBrains.Annotations;
using Tauron.Application.AkkaNode.Services.CleanUp.Core;
using Tauron.Application.Workshop.StateManagement;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.StatePooling;
using Tauron.Features;

namespace Tauron.Application.AkkaNode.Services.CleanUp
{
    [State]
    [DefaultDispatcher]
    public sealed class CleanUpManager : ActorFeatureStateBase<EmptyState>, IState<ToDeleteRevision>, IState<CleanUpTime>, IPooledState, IInitState<>
    {
        protected override void ConfigImpl()
        {

        }
    }
}