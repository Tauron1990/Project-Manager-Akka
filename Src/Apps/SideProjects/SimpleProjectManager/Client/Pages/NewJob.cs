using SimpleProjectManager.Client.ViewModels;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class NewJob
{
    protected override NewJobViewModel CreateModel()
        => Services.GetIsolatedService<NewJobViewModel>().DisposeWith(this);
}