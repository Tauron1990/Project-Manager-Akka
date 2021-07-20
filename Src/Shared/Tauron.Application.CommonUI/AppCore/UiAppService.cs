using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util;
using Autofac;
using Microsoft.Extensions.Hosting;

namespace Tauron.Application.CommonUI.AppCore
{
    public sealed class UiAppService : BackgroundService
    {
        private readonly ILifetimeScope _factory;
        private readonly CommonUIFramework _framework;
        private readonly ActorSystem _system;
        private readonly TaskCompletionSource<int> _shutdownWaiter = new();
        private readonly AtomicBoolean _shutdown = new(false);

        private IUIApplication? _internalApplication;

        public UiAppService(ILifetimeScope factory, CommonUIFramework framework, ActorSystem system)
        {
            _factory = factory;
            _framework = framework;
            _system = system;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            void ShutdownApp()
            {
                if(_shutdown.GetAndSet(true)) return;

                // ReSharper disable MethodSupportsCancellation
                Task.Run(async () =>
                         {
                             await Task.Delay(TimeSpan.FromSeconds(60));
                             Process.GetCurrentProcess().Kill(false);
                         });
                // ReSharper restore MethodSupportsCancellation
                _system.Terminate();
            }

            void Runner()
            {
                using var scope = _factory.BeginLifetimeScope();

                _internalApplication = scope.ResolveOptional<IAppFactory>()?.Create() ?? _framework.CreateDefault();
                _internalApplication.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                _internalApplication.Startup += (_, _) =>
                                                {
                                                    DispatcherScheduler.CurrentDispatcher = DispatcherScheduler.From(scope.Resolve<IUIDispatcher>());

                                                    // ReSharper disable AccessToDisposedClosure
                                                    var splash = scope.ResolveOptional<ISplashScreen>()?.Window;
                                                    splash?.Show();

                                                    var mainWindow = scope.Resolve<IMainWindow>();
                                                    mainWindow.Window.Show();
                                                    mainWindow.Shutdown += (_, _) => ShutdownApp();

                                                    splash?.Hide();
                                                    // ReSharper restore AccessToDisposedClosure
                                                };

                _system.RegisterOnTermination(() => _internalApplication.Dispatcher.Post(() => _internalApplication.Shutdown(0)));
                
                _shutdownWaiter.SetResult(_internalApplication.Run());
            }

            stoppingToken.Register(ShutdownApp);

            Thread uiThread = new(Runner)
                              {
                                  Name = "UI Thread",
                                  IsBackground = true
                              };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();

            return Task.CompletedTask;
        }
    }
}