using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Tauron.Application.Avalonia.AppCore;
using Tauron.Application.Avalonia.Dialogs;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using ShutdownMode = Tauron.Application.CommonUI.AppCore.ShutdownMode;

namespace Tauron.Application.Avalonia;

public sealed class AvaloniaFramework : CommonUIFramework
{
    public AvaloniaFramework() => NeedFullSync = true;

    public static IUIDispatcher UIDispatcher
        => new AvaloniaDispatcher();

    public override IUIApplication CreateDefault() => (IUIApplication)AppBuilder
       .Configure<AvalonApp>()
       .UsePlatformDetect()
       .LogToTrace()
       .SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime())
       .Instance;

    public override IWindow CreateMessageDialog(string title, string message)
    {
        var window = new global::Avalonia.Controls.Window();
        window.Content = new MessageDialog(title, message, b => window.Close(b), true) { Margin = new Thickness(10) };
        window.SizeToContent = SizeToContent.WidthAndHeight;
        //window.Owner = ((IClassicDesktopStyleApplicationLifetime) global::Avalonia.Application.Current.ApplicationLifetime).MainWindow;
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.ShowInTaskbar = false;

        return new AvaloniaWindow(window);
    }

    public override object CreateDefaultMessageContent(string title, string message, Action<bool?>? result, bool canCnacel)
        => new MessageDialog(title, message, result, canCnacel);

    private sealed class AvaloniaDispatcher : IUIDispatcher
    {
        public void Post(Action action)
        {
            Dispatcher.UIThread.Post(action);
        }

        public Task InvokeAsync(Action action) => Dispatcher.UIThread.InvokeAsync(action);

        public IObservable<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action)
            => Dispatcher.UIThread.InvokeAsync(action).ToObservable();

        public IObservable<TResult> InvokeAsync<TResult>(Func<TResult> action)
            => Dispatcher.UIThread.InvokeAsync(action).ToObservable();

        public bool CheckAccess() => Dispatcher.UIThread.CheckAccess();
    }

    private sealed class AvalonApp : global::Avalonia.Application, IUIApplication
    {
        #pragma warning disable GU0073
        public AvalonApp() => AppDispatcher = new AvaloniaDispatcher();
        #pragma warning restore GU0073

        private ClassicDesktopStyleApplicationLifetime AppLifetime
            => ApplicationLifetime as ClassicDesktopStyleApplicationLifetime ?? throw new InvalidOperationException("Only Classic Desktop Style Supported");


        public event EventHandler? Startup;

        public ShutdownMode ShutdownMode
        {
            get
            {
                return AppLifetime.ShutdownMode switch
                {
                    global::Avalonia.Controls.ShutdownMode.OnLastWindowClose => ShutdownMode.OnLastWindowClose,
                    global::Avalonia.Controls.ShutdownMode.OnMainWindowClose => ShutdownMode.OnMainWindowClose,
                    global::Avalonia.Controls.ShutdownMode.OnExplicitShutdown => ShutdownMode.OnExplicitShutdown,
                    _ => throw new InvalidCastException("Shutdown mode"),
                };
            }
            set
            {
                AppLifetime.ShutdownMode = value switch
                {
                    ShutdownMode.OnLastWindowClose => global::Avalonia.Controls.ShutdownMode.OnLastWindowClose,
                    ShutdownMode.OnMainWindowClose => global::Avalonia.Controls.ShutdownMode.OnMainWindowClose,
                    ShutdownMode.OnExplicitShutdown => global::Avalonia.Controls.ShutdownMode.OnExplicitShutdown,
                    _ => throw new InvalidCastException("Shutdown mode"),
                };
            }
        }

        public IUIDispatcher AppDispatcher { get; }

        public void Shutdown(int returnValue)
        {
            AppDispatcher.Post(() => { AppLifetime.Shutdown(returnValue); });
        }

        public int Run()
        {
            AppLifetime.Startup += (_, args) => Startup?.Invoke(this, args);

            return AppLifetime.Start(Environment.GetCommandLineArgs());
        }
    }

    public class DelegateAvalonApp : IUIApplication
    {
        private readonly global::Avalonia.Application _application;

        public DelegateAvalonApp(global::Avalonia.Application application)
        {
            _application = application;
            AppDispatcher = new AvaloniaDispatcher();
        }

        private ClassicDesktopStyleApplicationLifetime AppLifetime
            => _application.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime ?? throw new InvalidOperationException("Only Classic Desktop Style Supported");


        public event EventHandler? Startup;

        public ShutdownMode ShutdownMode
        {
            get
            {
                return AppLifetime.ShutdownMode switch
                {
                    global::Avalonia.Controls.ShutdownMode.OnLastWindowClose => ShutdownMode.OnLastWindowClose,
                    global::Avalonia.Controls.ShutdownMode.OnMainWindowClose => ShutdownMode.OnMainWindowClose,
                    global::Avalonia.Controls.ShutdownMode.OnExplicitShutdown => ShutdownMode.OnExplicitShutdown,
                    _ => throw new InvalidCastException("Shutdown mode"),
                };
            }
            set
            {
                AppLifetime.ShutdownMode = value switch
                {
                    ShutdownMode.OnLastWindowClose => global::Avalonia.Controls.ShutdownMode.OnLastWindowClose,
                    ShutdownMode.OnMainWindowClose => global::Avalonia.Controls.ShutdownMode.OnMainWindowClose,
                    ShutdownMode.OnExplicitShutdown => global::Avalonia.Controls.ShutdownMode.OnExplicitShutdown,
                    _ => throw new InvalidCastException("Shutdown mode"),
                };
            }
        }

        public IUIDispatcher AppDispatcher { get; }

        public void Shutdown(int returnValue)
        {
            AppDispatcher.Post(() => { AppLifetime.Shutdown(returnValue); });
        }

        public int Run()
        {
            AppLifetime.Startup += (_, args) => Startup?.Invoke(this, args);

            return AppLifetime.Start(Environment.GetCommandLineArgs());
        }
    }
}