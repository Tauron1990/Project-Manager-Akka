using System;

namespace Tauron.Application.CommonUI.Dialogs;

public interface IDialogCoordinatorUIEvents
{
    #pragma warning disable MA0046
    event Action<object>? ShowDialogEvent;
    

    event Action? HideDialogEvent;
}