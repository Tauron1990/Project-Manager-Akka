using System;
using System.IO;
using Tauron.Application.Localizer.DataModel.Processing.Messages;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectLoader : ObservableActor
    {
        public ProjectLoader()
        {
            Receive<InternalLoadProject>(obs => obs.SubscribeWithStatus(LoadProjectFile));
        }

        private void LoadProjectFile(InternalLoadProject obj)
        {
            var (loadProjectFile, originalSender) = obj;
            try
            {
                using var stream = File.OpenRead(loadProjectFile.Source);
                using var reader = new BinaryReader(stream);
                var projectFile = ProjectFile.ReadFile(reader, loadProjectFile.Source, Sender);

                originalSender.Tell(new LoadedProjectFile(loadProjectFile.OperationId, projectFile, null, Ok: true));
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