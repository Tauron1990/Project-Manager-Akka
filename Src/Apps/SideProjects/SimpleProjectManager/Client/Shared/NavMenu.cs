using Microsoft.AspNetCore.Components;

namespace SimpleProjectManager.Client.Shared;

public partial class NavMenu
{
    //private static string AdminTooltipContent = "Seiten die für Fehlerbehandlung und Server Administration gedacht sind";
    
    [Parameter]
    public EventCallback Navigating { get; set; }

    private async Task Navigate(string target)
    {
        await Navigating.InvokeAsync();
        NavigationManager.NavigateTo(target);
    }
}