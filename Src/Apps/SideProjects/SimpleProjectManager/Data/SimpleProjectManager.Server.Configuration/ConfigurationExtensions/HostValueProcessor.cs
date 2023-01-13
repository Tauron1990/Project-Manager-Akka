using System.Collections.Immutable;

namespace SimpleProjectManager.Server.Configuration.ConfigurationExtensions;

public class HostValueProcessor : ConfigExtension
{
    public override ImmutableDictionary<string, string> ProcessValue(ImmutableDictionary<string, string> settings, string key, string value)
        => string.Equals(key, "host", StringComparison.Ordinal)
            ? value switch
            {
                "local" when settings.ContainsKey("ip") => settings.SetItem("ip", "localhost"),
                _ => settings,
            }
            : settings;
}