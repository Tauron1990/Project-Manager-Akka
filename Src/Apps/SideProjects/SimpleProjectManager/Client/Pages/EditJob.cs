using Microsoft.AspNetCore.Components;
using SimpleProjectManager.Client.ViewModels;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class EditJob
{
    [Parameter]
    public string ProjectId { get; set; } = string.Empty;

    protected override EditJobViewModel CreateModel()
        => Services.GetIsolatedService<EditJobViewModel>().DisposeWith(this);
}