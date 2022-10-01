using SimpleProjectManager.Operation.Client.Config;

namespace SimpleProjectManager.Operation.Client.ImageEditor;

public sealed class ImageEditorStartup
{
    private readonly OperationConfiguration _configuration;

    public ImageEditorStartup(OperationConfiguration configuration)
        => _configuration = configuration;

    public void Run()
    {
        if(!_configuration.ImageEditor) return;
    }
}