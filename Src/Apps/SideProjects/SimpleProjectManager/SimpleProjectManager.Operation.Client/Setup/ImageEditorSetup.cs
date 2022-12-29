using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;
using Stl.IO;

namespace SimpleProjectManager.Operation.Client.Setup;

public sealed class ImageEditorSetup : ISetup
{
    private readonly IClientInteraction _clientInteraction;

    public ImageEditorSetup(IClientInteraction clientInteraction)
        => _clientInteraction = clientInteraction;

    public async ValueTask<OperationConfiguration> RunSetup(OperationConfiguration operationConfiguration)
    {
        bool isImageEditor = await _clientInteraction.Ask(operationConfiguration.Editor.Active, "Werden auf dem PC Bilder bearbeitet").ConfigureAwait(false);

        if(!isImageEditor)
            return operationConfiguration with { Editor = operationConfiguration.Editor with { Active = false, Path = FilePath.Empty } };

        string filePath = await _clientInteraction.AskForFile(operationConfiguration.Editor.Path.Value.EmptyToNull(), "Pfad zum Bild Editor").ConfigureAwait(false);

        return operationConfiguration with { Editor = operationConfiguration.Editor with { Active = true, Path = new FilePath(filePath) } };

    }
}