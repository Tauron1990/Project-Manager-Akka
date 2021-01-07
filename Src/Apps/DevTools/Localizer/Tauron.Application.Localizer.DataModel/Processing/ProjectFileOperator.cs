using System.Reactive.Linq;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing.Actors;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ProjectFileOperator : ExpandedReceiveActor
    {
        public ProjectFileOperator()
        {
            WhenReceiveSafe<LoadProjectFile>(observable => observable.Select(m => new InternalLoadProject(m, Context.Sender)).ToActor(c => c.ActorOf<ProjectLoader>()));
            WhenReceiveSafe<SaveProject>(obs => obs.ForwardToActor(c => c.GetOrAdd<ProjectSaver>("Saver")));
            WhenReceiveSafe<BuildRequest>(obs => obs.ForwardToActor(c => c.GetOrAdd<BuildActorCoordinator>("Builder")));
        }
    }
}