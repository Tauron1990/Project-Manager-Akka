using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Autofac;
using Tauron.Host;

namespace Tauron.Application.CommonUI.AppCore
{
    public sealed class AppLifetime : IAppRoute
    {
        private readonly ILifetimeScope _factory;
        private readonly CommonUIFramework _framework;
        private readonly TaskCompletionSource<int> _shutdownWaiter = new();
        private IUIApplication? _internalApplication;

        public AppLifetime(ILifetimeScope factory, CommonUIFramework framework)
        {
            _factory = factory;
            _framework = framework;
        }

        public Task WaitForStartAsync(ActorSystem system)
        {
            void ShutdownApp()
            {
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(60));
                    Process.GetCurrentProcess().Kill(false);
                });
                system.Terminate();
            }

            void Runner()
            {
                using var scope = _factory.BeginLifetimeScope();

                _internalApplication = scope.ResolveOptional<IAppFactory>()?.Create() ?? _framework.CreateDefault();
                _internalApplication.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                _internalApplication.Startup += (_, _) =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    var splash = scope.ResolveOptional<ISplashScreen>()?.Window;
                    splash?.Show();

                    var mainWindow = scope.Resolve<IMainWindow>();
                    mainWindow.Window.Show();
                    mainWindow.Shutdown += (_, _)
                        => ShutdownApp();

                    splash?.Hide();
                    // ReSharper restore AccessToDisposedClosure
                };

                system.RegisterOnTermination(() => _internalApplication.Dispatcher.Post(() => _internalApplication.Shutdown(0)));

                _shutdownWaiter.SetResult(_internalApplication.Run());
            }

            Thread uiThread = new(Runner)
            {
                Name = "UI Thread",
                IsBackground = true
            };
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            return Task.CompletedTask;
        }

        public Task ShutdownTask => _shutdownWaiter.Task;
    }
}