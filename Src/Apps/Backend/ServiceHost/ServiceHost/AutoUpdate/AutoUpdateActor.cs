using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using Tauron;
using Tauron.Features;
using Tauron.ObservableExt;

namespace ServiceHost.AutoUpdate
{
    [UsedImplicitly]
    public sealed class AutoUpdateActor : ActorFeatureBase<EmptyState>
    {
        private const string UpdaterExe = "AutoUpdateRunner.exe";
        private static readonly string UpdatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron", "PMHost");
        private static readonly string UpdateZip = Path.Combine(UpdatePath, "update.zip");

        public static Func<IPreparedFeature> New()
            => () => Feature.Create(() => new AutoUpdateActor());

        protected override void ConfigImpl()
        {
            Receive<StartAutoUpdate>(
                obs => obs.CatchSafe(
                    i => (
                        from request in Observable.Return(i.Event)
                                                  .Do(_ => Log.Info("Try Start Auto Update"))
                                                  .Do(_ => UpdatePath.CreateDirectoryIfNotExis())
                                                  .Do(r => File.Move(r.OriginalZip, UpdateZip, true))
                        let hostPath = new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty).LocalPath
                        let autoUpdateExe = Path.Combine(UpdatePath, UpdaterExe)
                        let info = new SetupInfo(UpdateZip, "ServiceHost.exe", hostPath, Environment.ProcessId, 5000)
                        select (hostPath, autoUpdateExe, info, exis:hostPath.ExisDirectory())
                    ).ConditionalSelect()
                     .ToResult<Unit>(
                          b =>
                          {
                              b.When(m => !m.exis, o => o.Do(_ => Log.Warning("Host Path Location not Found"))
                                                                             .ToUnit());

                              b.When(m => m.exis, 
                                  o => o.Do(m => File.Copy(Path.Combine(m.hostPath, UpdaterExe), m.autoUpdateExe, true))
                                        .Do(m => Process.Start(new ProcessStartInfo(m.autoUpdateExe, m.info.ToCommandLine()) { WorkingDirectory = UpdatePath}))
                                        .Do(_ => Context.System.Terminate())
                                        .ToUnit());
                          }),
                    (_, exception) => Observable.Return(Unit.Default)
                                                .Do(_ => Log.Warning(exception, "Error on Start Auto Update"))));

            Receive<StartCleanUp>(
                obs => obs.CatchSafe(
                    p => Observable.Return(Unit.Default)
                                   .Do(_ => Log.Info("Cleanup after Auto Update"))
                                   .Do(_ => KillProcess(p.Event.Id))
                                   .Do(_ => UpdateZip.DeleteFile())
                                   .Do(_ => UpdatePath.DeleteDirectory(true)),
                    (_, e) => Observable.Return(Unit.Default)
                                        .Do(_ => Log.Error(e, "Error on Cleanup Auto Update Files"))));
        }

        private void KillProcess(int id)
        {
            Log.Info("Killing Update process");
            try
            {
                using var process = Process.GetProcessById(id);
                var time = 60000;

                while (!process.HasExited)
                {
                    Thread.Sleep(1000);
                    time -= 1000;

                    if (time >= 0) continue;

                    process.Kill(true);
                    break;
                }
            }
            catch (ArgumentException)
            {
            }
            catch (Exception e)
            {
                Log.Error(e, "Error on Getting Update Process");
            }
        }
    }
}