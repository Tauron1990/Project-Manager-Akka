using MudBlazor;
using SimpleProjectManager.Shared;
using Tauron.Application.Blazor;

namespace SimpleProjectManager.Client.Pages;

public partial class FileManager
{
    private readonly TableGroupDefinition<DatabaseFile> _groupDefinition = new(file => file.JobName)
                                                                           {
                                                                               GroupName = "Job",
                                                                               Expandable = true,
                                                                               IsInitiallyExpanded = false,
                                                                           };

    private Action<DatabaseFile>? _deleteFile;

    protected override IEnumerable<IDisposable> InitializeModel()
    {
        if(ViewModel is null) yield break;

        _deleteFile = ViewModel.DeleteFile?.ToAction();

        yield return ViewModel.ConfirmDelete.RegisterHandler(
            async c =>
            {
                bool? result = await DialogService.ShowMessageBox(
                    new MessageBoxOptions
                    {
                        Title = "Datei Löschen",
                        Message = $"Möchten sie die datei {c.Input.Name} wirklich Löschen?",
                        YesText = "Ja",
                        NoText = "Nein",
                    },
                    new DialogOptions
                    {
                        DisableBackdropClick = true,
                        Position = DialogPosition.Center,
                    }
                );

                c.SetOutput(result ?? false);
            });
    }
}