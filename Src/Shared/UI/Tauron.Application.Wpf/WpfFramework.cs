using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.Wpf.AppCore;
using Tauron.Application.Wpf.Dialogs;
using ShutdownMode = Tauron.Application.CommonUI.AppCore.ShutdownMode;

namespace Tauron.Application.Wpf;

public sealed class WpfFramework : CommonUIFramework
{
    public WpfFramework() => NeedFullSync = false;

    public static IUIDispatcher Dispatcher(Dispatcher dispatcher) => new InternalDispatcher(dispatcher);

    public override IUIApplication CreateDefault() => new DelegateApplication(new System.Windows.Application());

    public override IWindow CreateMessageDialog(string title, string message)
    {
        var window = new System.Windows.Window();
        window.Content = new MessageDialog(title, message, b => window.DialogResult = b, true)
                         { Margin = new Thickness(10) };

        window.SizeToContent = SizeToContent.WidthAndHeight;
        window.ResizeMode = ResizeMode.NoResize;
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.WindowStyle = WindowStyle.ToolWindow;
        window.ShowInTaskbar = false;

        return new WpfWindow(window);
    }

    public override object CreateDefaultMessageContent(
        string title, string message, Action<bool?>? result,
        bool canCnacel) => new MessageDialog(title, message, result, canCnacel);

    private sealed class InternalDispatcher : IUIDispatcher
    {
        private readonly Dispatcher _dispatcher;

        internal InternalDispatcher(Dispatcher dispatcher) => _dispatcher = dispatcher;

        public void Post(Action action)
        {
            _dispatcher.InvokeAsync(action);
        }

        public Task InvokeAsync(Action action) => _dispatcher.InvokeAsync(action).Task;

        public IObservable<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action)
        {
            async Task<TResult> Invoker() => await await _dispatcher.InvokeAsync(action).Task;

            return Invoker().ToObservable();
        }

        public IObservable<TResult> InvokeAsync<TResult>(Func<TResult> action)
            => _dispatcher.InvokeAsync(action).Task.ToObservable();

        public bool CheckAccess() => _dispatcher.CheckAccess();
    }

    public sealed class DelegateApplication : IUIApplication
    {
        private readonly System.Windows.Application _application;

        public DelegateApplication(System.Windows.Application application)
        {
            _application = application;
            AppDispatcher = new InternalDispatcher(application.Dispatcher);
            application.Startup += (_, _) => OnStartup();
        }


        public event EventHandler? Startup;

        public ShutdownMode ShutdownMode
        {
            get => _application.ShutdownMode switch
            {
                System.Windows.ShutdownMode.OnLastWindowClose => ShutdownMode.OnLastWindowClose,
                System.Windows.ShutdownMode.OnMainWindowClose => ShutdownMode.OnMainWindowClose,
                System.Windows.ShutdownMode.OnExplicitShutdown => ShutdownMode.OnExplicitShutdown,
                _ => throw new InvalidCastException("Shutdown mode not Convertible")
            };
            set
            {
                _application.ShutdownMode = value switch
                {
                    ShutdownMode.OnLastWindowClose => System.Windows.ShutdownMode.OnLastWindowClose,
                    ShutdownMode.OnMainWindowClose => System.Windows.ShutdownMode.OnMainWindowClose,
                    ShutdownMode.OnExplicitShutdown => System.Windows.ShutdownMode.OnExplicitShutdown,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
            }
        }

        public IUIDispatcher AppDispatcher { get; }

        public void Shutdown(int returnValue)
        {
            _application.Shutdown(returnValue);
        }

        public int Run() => _application.Run();

        private void OnStartup()
            => Startup?.Invoke(this, EventArgs.Empty);
    }
}