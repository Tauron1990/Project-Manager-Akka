using Microsoft.AspNetCore.Components;

namespace SimpleProjectManager.Client.Shared;

public partial class NavMenu 
{
    [Parameter]
    public EventCallback Navigating { get; set; }

    private async Task Navigate(string target)
    {
        await Navigating.InvokeAsync();
        _navigationManager.NavigateTo(target);
    }
}