using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public record InternalLoadProject(LoadProjectFile ProjectFile, IActorRef OriginalSender);
}