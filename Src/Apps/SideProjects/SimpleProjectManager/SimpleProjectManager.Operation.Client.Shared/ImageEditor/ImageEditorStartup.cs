using SimpleProjectManager.Operation.Client.Shared.Config;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Operation.Client.Shared.ImageEditor;

public sealed class ImageEditorStartup : IStartUpAction
{
    private readonly ConfigManager _configManager;

    public ImageEditorStartup(ConfigManager configManager)
        => _configManager = configManager;

    public void Run()
    {
        if(!_configManager.Configuration.ImageEditor) return;
    }
}