using Akka.Actor;
using ServiceHost.ClientApp.Shared.ConfigurationServer.Data;

namespace ServiceHost.ClientApp.Shared.ConfigurationServer.Events;

public abstract record ConfigDataEvent(ConfigDataAction Action) : IConfigEvent;

public sealed record SeedDataEvent(SeedUrl SeedUrl, ConfigDataAction Action) : ConfigDataEvent(Action)
{
    public static IConfigEvent Create(SeedUrl url, ConfigDataAction action)
        => new SeedDataEvent(url, action);
}

public sealed record GlobalConfigEvent(GlobalConfig Config, ConfigDataAction Action) : ConfigDataEvent(Action)
{
    public static IConfigEvent Create(GlobalConfig config, ConfigDataAction action)
        => new GlobalConfigEvent(config, action);
}

public sealed record SpecificConfigEvent(SpecificConfig Config, ConfigDataAction Action) : ConfigDataEvent(Action)
{
    public static IConfigEvent Create(SpecificConfig config, ConfigDataAction action)
        => new SpecificConfigEvent(config, action);
}

public sealed record ConditionUpdateEvent(SpecificConfig Config, Condition Condition, string Id, ConfigDataAction Action) : ConfigDataEvent(Action)
{
    public static IConfigEvent Create(SpecificConfig config, Condition condition, string id, ConfigDataAction action)
        => new ConditionUpdateEvent(config, condition, id, action);
}

public sealed record HostUpdatedEvent(string Name, ActorPath Target) : IConfigEvent;