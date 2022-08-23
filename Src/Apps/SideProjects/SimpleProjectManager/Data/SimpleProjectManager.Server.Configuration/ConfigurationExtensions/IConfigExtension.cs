using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Configuration.ConfigurationExtensions;

public interface IConfigExtension
{
    void Apply(ImmutableDictionary<string, string> propertys, IActorApplicationBuilder applicationBuilder);
    string ProcessValue(ImmutableDictionary<string, string> settings, string key, string value);
}