using Akka.Actor;
using System;
using System.IO;
using Tauron.Application.Localizer.DataModel.Processing.Messages;
using Tauron.TAkka;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectLoader : ObservableActor
    {
        public ProjectLoader() => Receive<InternalLoadProject>(obs => obs.SubscribeWithStatus(LoadProjectFile));

        private void LoadProjectFile(InternalLoadProject obj)
        {
            (LoadProjectFile loadProjectFile, IActorRef originalSender) = obj;
            try
            {
                using FileStream stream = File.OpenRead(loadProjectFile.Source);
                using var reader = new BinaryReader(stream);
                ProjectFile projectFile = ProjectFile.ReadFile(reader, loadProjectFile.Source, Sender);

                originalSender.Tell(new LoadedProjectFile(loadProjectFile.OperationId, projectFile, ErrorReason: null, Ok: true));
            }
            catch (Exception e)
            {
                originalSender.Tell(
                    new LoadedProjectFile(
                        loadProjectFile.OperationId,
                        ProjectFile.FromSource(loadProjectFile.Source, Sender),
                        e,
                        Ok: false));
            }
            finally
            {
                Context.Stop(Self);
            }
        }
    }
}