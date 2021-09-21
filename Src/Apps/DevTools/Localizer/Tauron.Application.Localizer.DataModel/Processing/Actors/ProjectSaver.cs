using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Tauron.Application.Localizer.DataModel.Processing.Messages;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectSaver : ReceiveActor, IWithTimers
    {
        private readonly List<(SaveProject toSave, IActorRef Sender)> _toSave = new();
        private bool _sealed;

        public ProjectSaver()
        {
            Receive<SaveProject>(SaveProject);
            Receive<InitSave>(StartNormalSave);
            Receive<ForceSave>(TryForceSave);

            Timers!.StartSingleTimer(nameof(InitSave), new InitSave(), TimeSpan.FromSeconds(1));
        }

        public ITimerScheduler Timers { get; set; }

        private void TryForceSave(ForceSave obj)
        {
            Timers.Cancel(nameof(InitSave));
            StartNormalSave(null);

            var (andSeal, projectFile) = obj;
            var saveExcetion = TrySave(projectFile);
            if (saveExcetion != null)
                Context.GetLogger().Error(saveExcetion, "Error on force Saving Project");

            if (!andSeal) return;

            _sealed = true;
            Timers.CancelAll();
        }

        private void StartNormalSave(InitSave? obj)
        {
            if (_toSave.Count > 1)
                foreach (var (toSave, sender) in _toSave.Take(_toSave.Count - 1))
                    sender.Tell(new SavedProject(toSave.OperationId, Ok: true, null));

            if (_toSave.Count > 0)
            {
                var ((operationId, projectFile), send) = _toSave[^1];
                var result = TrySave(projectFile);
                send.Tell(new SavedProject(operationId, result is null, result));
            }

            Timers.Cancel(nameof(InitSave));
            Timers.StartSingleTimer(nameof(InitSave), new InitSave(), TimeSpan.FromSeconds(1));
        }

        private void SaveProject(SaveProject obj)
        {
            if (_sealed) return;

            _toSave.Add((obj, Sender));
        }

        private static Exception? TrySave(ProjectFile file)
        {
            try
            {
                if (file.Source.ExisFile())
                    File.Copy(file.Source, file.Source + ".bak", overwrite: true);
                using var stream = File.Open(file.Source, FileMode.Create);
                using var writer = new BinaryWriter(stream);
                file.Write(writer);

                return null;
            }
            catch (Exception e)
            {
                #pragma warning disable ERP022
                return e;
                #pragma warning restore ERP022
            }
        }

        private sealed class InitSave { }
    }
}