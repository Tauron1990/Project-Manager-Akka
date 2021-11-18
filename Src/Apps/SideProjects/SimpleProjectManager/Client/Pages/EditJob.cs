using Microsoft.AspNetCore.Components;

namespace SimpleProjectManager.Client.Pages;

public partial class EditJob
{
    [Parameter]
    public string ProjectId { get; set; } = string.Empty;
}