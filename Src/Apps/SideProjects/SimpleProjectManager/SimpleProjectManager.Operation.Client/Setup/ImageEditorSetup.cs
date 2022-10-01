using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;

namespace SimpleProjectManager.Operation.Client.Setup;

public sealed class ImageEditorSetup : ISetup
{
    private readonly IClientInteraction _clientInteraction;

    public ImageEditorSetup(IClientInteraction clientInteraction)
        => _clientInteraction = clientInteraction;

    public async ValueTask<OperationConfiguration> RunSetup(OperationConfiguration operationConfiguration)
    {
        var isImageEditor = await _clientInteraction.Ask(operationConfiguration.ImageEditor, "Werden auf dem PC Bilder bearbeitet");

        if(!isImageEditor)
            return operationConfiguration with { ImageEditor = false, Path = string.Empty };

        var filePath = await _clientInteraction.AskForFile(operationConfiguration.Path.EmptyToNull(), "Pfad zum Bild Editor");

        return operationConfiguration with { ImageEditor = true, Path = filePath };

    }
}