using System.Reactive.Linq;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing.Actors;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ProjectFileOperator : ExpandedReceiveActor
    {
        public ProjectFileOperator()
        {
            WhenReceiveSafe<LoadProjectFile>(observable => observable.Select(m => new InternalLoadProject(m, Context.Sender)).ToActor(() => Context.ActorOf<ProjectLoader>()));

            Flow<LoadProjectFile>(b => b.Func(lp => new InternalLoadProject(lp, Context.Sender)).ToRef(ac => ac.ActorOf<ProjectLoader>()));

            Flow<SaveProject>(b => b.External(c => c.GetOrAdd<ProjectSaver>("Saver"), true));

            Flow<BuildRequest>(b => b.External(c => c.GetOrAdd<BuildActorCoordinator>("Builder"), true));
        }
    }
}