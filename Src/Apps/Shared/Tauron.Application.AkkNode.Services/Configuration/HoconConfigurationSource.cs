using System;
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