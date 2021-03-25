using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Dialogs
{
    public interface IDialogCoordinatorUIEvents
    {
        event Action<object>? ShowDialogEvent;

        event Action? HideDialogEvent;
    }


    [PublicAPI]
    public interface IDialogCoordinator
    {
        //event Action<IWindow>? OnWindowConstructed;

        Task<bool?> ShowMessage(string title, string message, Action<bool?>? result);

        void ShowMessage(string title, string message);
        void ShowDialog(object dialog);

        void HideDialog();
        //Task<bool?> ShowModalMessageWindow(string title, string message);
    }
}