using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using SimpleProjectManager.Shared;
using Tauron;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class FileDetailDisplay
{
    [Parameter]
    public ProjectFileId? FileId { get; set; }

    protected override FileDetailDisplayViewModel CreateModel()
        => Services.GetIsolatedService<FileDetailDisplayViewModel>();

    protected override void InitializeModel()
    {
        this.WhenActivated(
            dispo =>
            {
                if(ViewModel == null) return;

                ViewModel.Id.RegisterHandler(context => context.SetOutput(FileId))
                   .DisposeWith(dispo);
            });
    }
}