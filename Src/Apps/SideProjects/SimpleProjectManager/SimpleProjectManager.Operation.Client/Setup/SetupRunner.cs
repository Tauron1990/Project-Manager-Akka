using System.Collections.Immutable;
using FluentValidation.Results;
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
                      new ImageEditorSetup(clientInteraction),
                  };
    }

    public OperationConfiguration Configuration => _configManager.Configuration;

    public async ValueTask RunSetup(ClientConfiguration commandLine)
    {
        OperationConfiguration config = _configManager.Configuration;
        var validation = await config.Validate().ConfigureAwait(false);
        if(CheckUseConfiguration(validation, commandLine, config))
            return;

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (ISetup setup in _setups)
            config = await setup.RunSetup(config).ConfigureAwait(false);

        validation = await config.Validate().ConfigureAwait(false);

        if(!validation.IsEmpty)
            throw new InvalidOperationException(string.Join(", ", validation.Select(v => v.ErrorMessage)));

        await _configManager.Set(config).ConfigureAwait(false);
    }

    private bool CheckUseConfiguration(ImmutableList<ValidationFailure> validation, ClientConfiguration clientConfiguration, OperationConfiguration config)
    {
        if(clientConfiguration.ForceSetup)
            return false;

        if(validation.IsEmpty)
            return true;

        _clientInteraction.Display(validation);

        return false;
    }
}