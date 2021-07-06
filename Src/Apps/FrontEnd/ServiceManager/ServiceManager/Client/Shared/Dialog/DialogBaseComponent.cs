using Microsoft.AspNetCore.Components;
using MudBlazor;
using ServiceManager.Client.Components;

namespace ServiceManager.Client.Shared.Dialog
{
    public abstract class DialogBaseComponent : DisposableComponent
    {
        [CascadingParameter]
        protected MudDialogInstance DialogInstance { get; set; } = null!;
    }
}