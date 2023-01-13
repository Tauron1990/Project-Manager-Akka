using Microsoft.AspNetCore.Components;

namespace SimpleProjectManager.Client.Shared.AppBar;

public partial class MainAppBar
{
    [Parameter]
    public EventCallback ToggleDrawer { get; set; }
}