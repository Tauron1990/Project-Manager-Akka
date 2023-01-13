using System.Collections.Immutable;
using Microsoft.AspNetCore.Hosting;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Configuration.ConfigurationExtensions;

public abstract class ConfigExtension : IConfigExtension
{
    public virtual void Apply(ImmutableDictionary<string, string> propertys, IWebHostBuilder applicationBuilder) { }

    public virtual void Apply(ImmutableDictionary<string, string> propertys, IActorApplicationBuilder applicationBuilder) { }

    public virtual ImmutableDictionary<string, string> ProcessValue(ImmutableDictionary<string, string> settings, string key, string value)
        => settings;
}