using System.Diagnostics;
using System.Reactive.Linq;
using Akka.Actor;
using Akka.Event;
using IniParser;
using Microsoft.Extensions.Logging;
using Tauron;
using Tauron.Features;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager
{
    public sealed class MgiStartingActor : ActorFeatureBase<IActorRef>
    {
        private MgiStartingActor() { }

        public static IPreparedFeature New()
            => Feature.Create(() => new MgiStartingActor(), c => c.ActorOf("Process_Manager", ProcessManagerActor.New()));

        protected override void ConfigImpl()
        {
            Observ<TryStart>(obs => obs.Select<StatePair<TryStart, IActorRef>, TryStart>(p => p.Event).SubscribeWithStatus(TryStartHandler));

            Observ<RegisterProcessList>(obs => obs.Select(p => p.Event).ToActor(CurrentState));
        }

        private void TryStartHandler(TryStart obj)
        {
            try
            {
                Logger.LogInformation("Start Mgi Process");

                var config = obj.Config;
                obj.Kill();

                Thread.Sleep(500);
                if (!CheckKernelRunning(config, obj.Cancel, out var kernel))
                {
                    //CurrentState.ShowMessageBox(null, Context.Loc().RequestString("kernelstarterror"), "Error", MsgBoxButton.Ok, MsgBoxImage.Error);
                    try
                    {
                        kernel.Kill(entireProcessTree: true);
                        kernel.Dispose();
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e, "Error on Kill Kernel");
                    }

                    return;
                }
                
                Thread.Sleep(500);
                #pragma warning disable EX006
                Directory.SetCurrentDirectory(Path.GetDirectoryName(config.Client.Trim()) ?? throw new InvalidOperationException("Current Directory Set Fail"));
                #pragma warning restore EX006

                if (obj.Cancel.IsCancellationRequested)
                {
                    kernel.Kill(entireProcessTree: true);

                    return;
                }

                CurrentState.Tell(kernel);
                CurrentState.Tell(Process.Start(config.Client));
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Error on Start Mgi process");
            }
            finally
            {
                Sender.Tell(new TryStartResponse());
            }
        }

        private bool CheckKernelRunning(ProcessConfiguration config, CancellationToken token, out Process kernel)
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
                catch (Exception e)
                {
                    Context.GetLogger().Error(e, "Error on Check Kernel Running");
                    Thread.Sleep(500);
                }

            return false;
        }

        public sealed record TryStart(ProcessConfiguration Config, CancellationToken Cancel, Action Kill);

        public sealed record TryStartResponse;
    }
}