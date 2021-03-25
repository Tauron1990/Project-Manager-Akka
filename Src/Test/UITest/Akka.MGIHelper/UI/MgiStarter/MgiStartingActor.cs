using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using Akka.Actor;
using Akka.MGIHelper.Core.Configuration;
using IniParser;
using Tauron;
using Tauron.Application.Wpf;
using Tauron.Features;
using Tauron.Localization;

namespace Akka.MGIHelper.UI.MgiStarter
{
    public sealed class MgiStartingActor : ActorFeatureBase<IDialogFactory>
    {
        public static IPreparedFeature New(IDialogFactory dialogFactory)
            => Feature.Create(() => new MgiStartingActor(), dialogFactory);

        private MgiStartingActor(){}

        protected override void Config()
        {
            Receive<TryStart>(obs => obs.Select(p => p.Event).SubscribeWithStatus(TryStartHandler));
        }

        private void TryStartHandler(TryStart obj)
        {
            try
            {
                Log.Info("Start Mgi Process");

                var config = obj.Config;
                Sender.Tell(new StartStatusUpdate(Context.Loc().RequestString("kernelstartstartlabel")));
                obj.Kill();

                Thread.Sleep(500);
                if (!CheckKernelRunning(config, obj.Cancel, out var target))
                {
                    //CurrentState.ShowMessageBox(null, Context.Loc().RequestString("kernelstarterror"), "Error", MsgBoxButton.Ok, MsgBoxImage.Error);
                    try
                    {
                        target.Kill(true);
                        target.Dispose();
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error on Kill Kernel");
                    }

                    return;
                }

                Thread.Sleep(500);
                Directory.SetCurrentDirectory(Path.GetDirectoryName(config.Client.Trim()) ?? throw new InvalidOperationException("Current Directory Set Fail"));

                if (obj.Cancel.IsCancellationRequested)
                {
                    target.Kill(true);
                    return;
                }

                Process.Start(config.Client);

                Sender.Tell(new StartStatusUpdate(Context.Loc().RequestString("kernelstartcompledlabel")));
            }
            catch (Exception e)
            {
                Log.Warning(e, "Error on Start Mgi process");
                CurrentState.FormatException(null, e);
            }
            finally
            {
                Sender.Tell(new TryStartResponse());
            }
        }

        private bool CheckKernelRunning(ProcessConfig config, CancellationToken token, out Process kernel)
        {
            var kernelPath = config.Kernel.Trim();
            var statusPath = Path.Combine(Path.GetDirectoryName(kernelPath) ?? string.Empty, "Status.ini");
            const int iterationCount = 60;
            var parser = new FileIniDataParser();

            if (File.Exists(statusPath))
                File.Delete(statusPath);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(kernelPath) ?? throw new InvalidOperationException("Current Directory Set Fail"));
            kernel = Process.Start(kernelPath);

            if (token.IsCancellationRequested) return false;

            Thread.Sleep(5000);

            for (var i = 0; i < iterationCount; i++)
            {
                try
                {
                    if (token.IsCancellationRequested) return false;

                    Thread.Sleep(1100);

                    var data = parser.ReadFile(statusPath);
                    var status = data.Sections["Status"].GetKeyData("Global").Value;
                    switch (status)
                    {
                        case "Ready":
                            return true;
                        case "Fail":
                        case "Close":
                            return false;
                    }
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }

            return false;
        }

        public sealed record TryStart(ProcessConfig Config, CancellationToken Cancel, Action Kill);

        public sealed record TryStartResponse;

        public sealed record StartStatusUpdate(string Status);
    }
}