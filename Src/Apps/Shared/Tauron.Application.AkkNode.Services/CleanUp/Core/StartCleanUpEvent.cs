using Tauron.Application.Workshop.Mutating.Changes;

namespace Tauron.Application.AkkaNode.Services.CleanUp.Core
{
    public sealed record StartCleanUpEvent(bool Run) : MutatingChange;
}