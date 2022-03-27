using System.Reactive.Disposables;
using MudBlazor;
using ReactiveUI;
using SimpleProjectManager.Shared;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class FileManager
{
    private readonly TableGroupDefinition<DatabaseFile> _groupDefinition = new(file => file.JobName)
                                                                           {
                                                                               GroupName = "Job: ",
                                                                               Expandable = true,
                                                                               IsInitiallyExpanded = false
                                                                           };

    private Action<DatabaseFile> _deleteFile = _ => { };

    protected override void InitializeModel()
    {
        
        this.WhenActivated(
            dipo =>
            {
                if(ViewModel == null) return;

                _deleteFile = ViewModel.DeleteFile.ToAction();
                
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
                            
                            c.SetOutput(result ?? false);
                        })
                   .DisposeWith(dipo);
            });
        base.InitializeModel();
    }
}