using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Serilog;

namespace Tauron.Application.CommonUI.Dialogs
{
    [PublicAPI]
    public sealed class DialogCoordinator : IDialogCoordinator, IDialogCoordinatorUIEvents
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<DialogCoordinator>();
        private readonly CommonUIFramework _framework;

        public DialogCoordinator(CommonUIFramework framework) => _framework = framework;

        //public event Action<IWindow>? OnWindowConstructed;

        public Task<bool?> ShowMessage(string title, string message, Action<bool?>? result)
        {
            var resultTask = new TaskCompletionSource<bool?>();
            result = result.Combine(b =>
            {
                HideDialog();
                resultTask.SetResult(b);
            });

            ShowDialog(_framework.CreateDefaultMessageContent(title, message, result, true));

            return resultTask.Task;
        }

        public void ShowMessage(string title, string message)
        {
            ShowDialog(_framework.CreateDefaultMessageContent(title, message, _ => HideDialog(), false));
        }

        public void ShowDialog(object dialog)
        {
            Log.Information("Show Dialog {Type}", dialog.GetType());
            ShowDialogEvent?.Invoke(dialog);
        }

        public void HideDialog()
        {
            HideDialogEvent?.Invoke();
        }

        event Action<object>? IDialogCoordinatorUIEvents.ShowDialogEvent
        {
            add => ShowDialogEvent += value;
            remove => ShowDialogEvent -= value;
        }

        event Action? IDialogCoordinatorUIEvents.HideDialogEvent
        {
            add => HideDialogEvent += value;
            remove => HideDialogEvent -= value;
        }

        //public Task<bool?> ShowModalMessageWindow(string title, string message)
        //{
        //    var window = _framework.CreateMessageDialog(title, message);

        //    OnWindowConstructed?.Invoke(window);

        //    return window.ShowDialog();
        //}

        private event Action<object>? ShowDialogEvent;

        private event Action? HideDialogEvent;
    }
}