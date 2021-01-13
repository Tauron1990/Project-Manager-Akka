using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Akka.Actor;
using AnyConsole;
using Autofac.Features.OwnedInstances;
using Serilog;
using Tauron.Host;

namespace Tauron.Application.AkkaNode.Bootstrap.Console
{
    public sealed class ConsoleAppRoute : IAppRoute
    {
        private Owned<IEnumerable<IStartUpAction>>? _actions;
        private readonly ExtendedConsole _console;

        public ConsoleAppRoute(Owned<IEnumerable<IStartUpAction>> startUpActions, ExtendedConsole console)
        {
            _actions = startUpActions;
            _console = console;
        }

        public Task ShutdownTask { get; private set; } = Task.CompletedTask;
        public Task WaitForStartAsync(ActorSystem actorSystem)
        {
            var source = new TaskCompletionSource<object>();
            ShutdownTask = source.Task;

            actorSystem.RegisterOnTermination(() => source.SetResult(new object()));

            Maximize();
            Task.Run(() =>
            {
                _console.Start();

                if (_actions != null)
                {
                    foreach (var startUpAction in _actions.Value)
                    {
                        try
                        {
                            startUpAction.Run();
                        }
                        catch (Exception e)
                        {
                            Log.Logger.Error(e, "Error on Startup Action");
                        }
                    }
                }

                _actions?.Dispose();
                _actions = null;
            });
            return Task.CompletedTask;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        private static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }
    }
}