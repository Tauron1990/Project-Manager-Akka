using System.Collections.Immutable;
using FluentValidation.Results;
using Microsoft.Extensions.Configuration;
using SimpleProjectManager.Operation.Client.Config;
using SimpleProjectManager.Operation.Client.Core;

namespace SimpleProjectManager.Operation.Client.Setup;

public sealed class SetupRunner
{
    private readonly IClientInteraction _clientInteraction;

    private readonly ConfigManager _configManager;
    private readonly ISetup[] _setups;

    public SetupRunner(ConfigManager configManager, IClientInteraction clientInteraction)
    {
        _configManager = configManager;
        _clientInteraction = clientInteraction;

        _setups = new ISetup[]
                  {
                      new IpSetup(clientInteraction),
                      new DevicesSetup(clientInteraction),
                      new ImageEditorSetup(clientInteraction)
                  };
    }

    public OperationConfiguration Configuration => _configManager.Configuration;

    public async ValueTask RunSetup(IConfiguration commandLine)
    {
        bool skipConfig = commandLine.GetValue("skipconfig", false);
        OperationConfiguration config = _configManager.Configuration;
        var validation = await config.Validate();
        if(await AskUseConfiguration(validation, skipConfig, config))
            return;

        if(skipConfig)
            throw new InvalidOperationException("Die Setup Phase kann nicht mit einer Fehlerhaften Configuration übersprungen werden");

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (ISetup setup in _setups)
            config = await setup.RunSetup(config);

        validation = await config.Validate();

        if(!validation.IsEmpty)
            throw new InvalidOperationException(string.Join(", ", validation.Select(v => v.ErrorMessage)));

        await _configManager.Set(config);
    }

    private async Task<bool> AskUseConfiguration(ImmutableList<ValidationFailure> validation, bool skipConfig, OperationConfiguration config)
    {
        switch (validation.IsEmpty)
        {
            case true when !skipConfig:
            {
                bool shouldUse = await _clientInteraction.Ask(
                    (bool?)null,
                    $"Möchten sie diese Configuration Benutzen?:{Environment.NewLine}{config}");

                if(shouldUse)
                    return true;

                break;
            }
            case true:
                return true;
        }

        return false;
    }
}