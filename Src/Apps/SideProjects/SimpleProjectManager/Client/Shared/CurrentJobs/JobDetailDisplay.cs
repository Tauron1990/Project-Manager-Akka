using System.Reactive.Linq;
using ReactiveUI;
using Tauron;
using Tauron.Application.Blazor.Commands;
using System.Reactive.Disposables;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobDetailDisplay
{
    private MudCommandButton? _editButton;

    private MudCommandButton? EditButton
    {
        get => _editButton;
        set => this.RaiseAndSetIfChanged(ref _editButton, value);
    }

    protected override void InitializeModel()
    {
        this.WhenActivated(dispo =>
                           {
                               if(ViewModel == null) return;

                               this.BindCommand(
                                       ViewModel,
                                       m => m.EditJob,
                                       v => v.EditButton,
                                       ViewModel.WhenAny(m => m.JobData, m => m.Value))
                                  .DisposeWith(dispo);
                           });
    }

}