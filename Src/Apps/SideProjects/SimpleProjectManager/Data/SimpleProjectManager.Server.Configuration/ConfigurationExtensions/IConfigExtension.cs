using Microsoft.Extensions.Configuration;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Configuration.ConfigurationExtensions;

public interface IConfigExtension
{
    void Apply(IConfiguration configuration, IActorApplicationBuilder applicationBuilder);
    string ProcessValue(KeyValuePair<string, string> settings, string key, string value);
}