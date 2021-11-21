using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Shared.Services.Tasks;

namespace SimpleProjectManager.Client.Shared.Tasks;

public partial class PendingTaskDisplay
{
    [Parameter]
    public PendingTask? PendingTask { get; set; }
}