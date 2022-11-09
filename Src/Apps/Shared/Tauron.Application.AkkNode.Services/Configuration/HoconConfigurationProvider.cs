using System;
using System.Linq;
using Akka.Configuration;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;

namespace Tauron.Application.AkkaNode.Services.Configuration;

[PublicAPI]
public sealed class HoconConfigurationSource : IConfigurationSource
{
    private readonly Func<Config> _config;
    private readonly (string path, string name)[] _names;

    public HoconConfigurationSource(Func<Config> config, params (string path, string name)[] names)
    {
        _config = config;
        _names = names;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new HoconConfigurationProvider(_config, _names);
}

public sealed class HoconConfigurationProvider : ConfigurationProvider
{
    private Func<Config>? _config;
    private (string path, string name)[]? _names;

    public HoconConfigurationProvider(Func<Config> config, params (string path, string name)[] names)
    {
        _config = config;
        _names = names;
    }

    public override void Load()
    {
        if(_config is null || _names is null)
            return;

        Config config = _config();

        foreach ((string path, string name) in _names.Where(p => config.HasPath(p.path)))
            Data[name] = config.GetString(path, string.Empty);

        _names = null;
        _config = null;
    }
}