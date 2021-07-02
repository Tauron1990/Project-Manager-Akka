using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ServiceManager.Client.Shared.Dialog
{
    public abstract class DialogBaseComponent : ComponentBase
    {
        [CascadingParameter] 
        protected MudDialogInstance DialogInstance { get; set; }
    }
}