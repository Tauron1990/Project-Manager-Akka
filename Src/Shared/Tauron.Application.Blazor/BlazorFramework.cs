using System;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Tauron.Application.Blazor.InternalComponents;
using Tauron.Application.CommonUI;
using Tauron.Application.CommonUI.AppCore;

namespace Tauron.Application.Blazor
{
    public sealed class BlazorFramework : CommonUIFramework
    {
        public static readonly IUIDispatcher Dispatcher = new FakeDispatcher();

        public override IUIApplication CreateDefault() => new FakeApp();

        public override IWindow CreateMessageDialog(string title, string message) => throw new NotSupportedException("No Window Support in Blazor");

        #pragma warning disable BL0005
        public override object CreateDefaultMessageContent(string title, string message, Action<bool?>? result, bool canCnacel)
            => new DefaultMessageContent { CanCancel = canCnacel, Content = message, ResultAction = result, Title = title };
        #pragma warning restore BL0005
        private sealed class FakeDispatcher : IUIDispatcher
        {
            public void Post(Action action) => action();

            public Task InvokeAsync(Action action) => Task.Run(action);

            public IObservable<TResult> InvokeAsync<TResult>(Func<Task<TResult>> action) => Task.Run(async () => await action()).ToObservable();

            public IObservable<TResult> InvokeAsync<TResult>(Func<TResult> action) => Task.Run(action).ToObservable();

            public bool CheckAccess() => true;
        }

        private sealed class FakeApp : IUIApplication
        {
            public ShutdownMode ShutdownMode
            {
                get => throw new NotSupportedException("No explicit App Support in Blazor");
                set => throw new NotSupportedException("No explicit App Support in Blazor");
            }

            public IUIDispatcher AppDispatcher => Dispatcher;

            public event EventHandler? Startup
            {
                add => throw new NotSupportedException("No explicit App Support in Blazor");
                remove => throw new NotSupportedException("No explicit App Support in Blazor");
            }

            public void Shutdown(int returnValue)
                => throw new NotSupportedException("No explicit App Support in Blazor");

            public int Run() => throw new NotSupportedException("No explicit App Support in Blazor");
        }
    }
}