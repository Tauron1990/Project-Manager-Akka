using Microsoft.Extensions.Configuration;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;

namespace SimpleProjectManager.Operation.Client.Setup;

public sealed class SetupRunner
{
    private readonly IClientInteraction _clientInteraction;
    private readonly ISetup[] _setups;

    private readonly ConfigManager _configManager;

    public OperationConfiguration Configuration => _configManager.Configuration;
    
    public SetupRunner(ConfigManager configManager, IClientInteraction clientInteraction)
    {
        _configManager = configManager;
        _clientInteraction = clientInteraction;
        
        _setups = new ISetup[]
                  {
                      new IpSetup(clientInteraction),
                      new DevicesSetup(clientInteraction),
                      new ImageEditorSetup(clientInteraction),
                  };
    }

    public async ValueTask RunSetup(IConfiguration commandLine)
    {
        var skipConfig = commandLine.GetValue("skipconfig", false);
        var config = _configManager.Configuration;
        var validation = await config.Validate();
        if(validation.IsEmpty)
        {
            if(!skipConfig)
            {
                var shouldUse = await _clientInteraction.Ask(
                    (bool?)null,
                    $"Möchten sie diese Configuration Benutzen?:{Environment.NewLine}{config}");

                if(shouldUse)
                    return;
            }
            else
                return;
        }

        if(skipConfig)
            throw new InvalidOperationException("Die Setup Phase kann nicht mit einer Fehlerhaften Configuration übersprungen werden");

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var setup in _setups)
            config = await setup.RunSetup(config);

        validation = await config.Validate();
        if(validation.IsEmpty)
        {
            await _configManager.Set(config);
            return;
        }

        throw new InvalidOperationException(string.Join(", ", validation.Select(v => v.ErrorMessage)));
    }
}