using System.Reactive;
using MudBlazor;
using Tauron.Application.Blazor;

namespace Tauron.Application.ServiceManager.Components.Dialog
{
    public sealed class ConfirmRestartDialog : GenericDialogContainer<bool, Unit, ConfirmRestartComponent>
    {
        public ConfirmRestartDialog(IDialogService service) 
            : base(service)
        {
        }
    }
}