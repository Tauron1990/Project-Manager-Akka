using System.Reactive.Linq;
using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Components;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using Tauron;
using Tauron.Application.Blazor;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobDetailDisplay
{
    private MudCommandButton? _editButton;

    protected override JobDetailDisplayViewModel CreateModel()
        => Services.GetRequiredService<JobDetailDisplayViewModel>();

    private MudCommandButton? EditButton
    {
        get => _editButton;
        set
        {
            _editButton = value;
            OnPropertyChanged();
        }
    }

    protected override void InitializeModel()
    {
        this.WhenActivated(dispo =>
                           {
                               if(ViewModel == null) return;

                               this.BindCommand(
                                       ViewModel,
                                       m => m.EditJobs,
                                       v => v.EditButton,
                                       ViewModel.NextElement.Select(d => d?.Id))
                                  .DisposeWith(dispo);
                           });
    }

}