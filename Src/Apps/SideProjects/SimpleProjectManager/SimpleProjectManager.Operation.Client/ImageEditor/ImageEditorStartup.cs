using Microsoft.Extensions.Logging;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;

namespace SimpleProjectManager.Operation.Client.ImageEditor;

public sealed class ImageEditorStartup
{
    private readonly OperationConfiguration _configuration;
    private readonly ILogger<ImageEditorStartup> _logger;
    private readonly HostStarter _starter;

    public ImageEditorStartup(HostStarter starter, OperationConfiguration configuration, ILogger<ImageEditorStartup> logger)
    {
        _starter = starter;
        _configuration = configuration;
        _logger = logger;
    }

    public async void Run()
    {
        try
        {
            if(!_configuration.Editor.Active) return;

            await _starter.NameRegistrated;
        }
        catch (Exception e)
        {
            //Later
        }
    }
}