using System.Reactive.Linq;
using Blazor.Extensions.Logging;
using ReactiveUI;
using SimpleProjectManager.Client.ViewModels;
using Tauron;
using Tauron.Application.Blazor;
using Tauron.Application.Blazor.Commands;

namespace SimpleProjectManager.Client.Shared.CurrentJobs;

public partial class JobDetailDisplay
{
    protected override JobDetailDisplayViewModel CreateModel()
    {
        _logger.LogInformation("Create Job Detail Diplay View Model");
        return Services.GetRequiredService<JobDetailDisplayViewModel>();
    }

    public MudCommandButton? EditButton { get; set; }
    
    protected override void InitializeModel()
    {
        _logger.LogInformation("intialize Job Detail Diplay View Model");

        this.WhenActivated(dispo =>
                           {
                               if(ViewModel == null) return;

                               _logger.LogInformation("Create Job Detail Command Bindings for View Model");

                               this.BindCommand(ViewModel, 
                                       m => m.EditJobs,
                                       v => v.EditButton,
                                       ViewModel.State.ToObservable().NotNull().Select(d => d.Id))
                                  .DisposeWith(dispo);
                           });
    }

}