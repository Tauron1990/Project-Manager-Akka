using System.Reactive.Disposables;
using MudBlazor;
using ReactiveUI;
using SimpleProjectManager.Shared;

namespace SimpleProjectManager.Client.Pages;

public partial class FileManager
{
    private readonly TableGroupDefinition<DatabaseFile> _groupDefinition = new(file => file.JobName)
                                                                           {
                                                                               GroupName = "Job: ",
                                                                               Expandable = true,
                                                                               IsInitiallyExpanded = false
                                                                           };

    protected override void InitializeModel()
    {
        
        this.WhenActivated(
            dipo =>
            {
                if(ViewModel == null) return;

                ViewModel.ConfirmDelete.RegisterHandler(
                        async c =>
                        {
                            var result = await _dialogService.ShowMessageBox
                            (
                                new MessageBoxOptions
                                {
                                    Title = "Datei Löschen",
                                    Message = $"Möchten sie die datei {c.Input.Name} wirklich Löschen?",
                                    YesText = "Ja",
                                    NoText = "Nein"
                                },
                                new DialogOptions
                                {
                                    DisableBackdropClick = true,
                                    Position = DialogPosition.Center
                                }
                            );
                        })
                   .DisposeWith(dipo);
            });
        base.InitializeModel();
    }
}