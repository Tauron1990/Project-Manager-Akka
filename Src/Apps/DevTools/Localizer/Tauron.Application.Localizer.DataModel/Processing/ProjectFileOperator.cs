using System.Reactive.Linq;
using Akka.Actor;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing.Actors;
using Tauron.Application.Localizer.DataModel.Processing.Messages;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ProjectFileOperator : ObservableActor
    {
        public ProjectFileOperator()
        {
            Receive<LoadProjectFile>(
                observable
                    => observable.Select(m => new InternalLoadProject(m, Context.Sender))
                       .ToActor(c => c.ActorOf<ProjectLoader>()));
            Receive<SaveProject>(obs => obs.ForwardToActor(c => c.GetOrAdd<ProjectSaver>("Saver")));
            Receive<BuildRequest>(obs => obs.ForwardToActor(c => c.GetOrAdd<BuildActorCoordinator>("Builder")));
        }
    }
}