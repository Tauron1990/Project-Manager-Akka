using System.Collections.Immutable;
using SimpleProjectManager.Server.Configuration.ConfigurationExtensions;

namespace SimpleProjectManager.Server.Configuration;

public sealed class StartConfigManager
{
    private ImmutableArray<IConfigExtension> _extensions = ImmutableArray<IConfigExtension>.Empty;

    public static StartConfigManager ConfigManager { get; } = new();

    public void RegisterExtension(IConfigExtension extension)
        => _extensions = _extensions.Add(extension);
}