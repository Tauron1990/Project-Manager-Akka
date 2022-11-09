using Akka.Actor;
using ServiceHost.Client.Shared.ConfigurationServer.Data;

namespace ServiceHost.Client.Shared.ConfigurationServer.Events;

public abstract record ConfigDataEvent(ConfigDataAction Action) : IConfigEvent;

public sealed record SeedDataEvent(SeedUrl SeedUrl, ConfigDataAction Action) : ConfigDataEvent(Action);

public sealed record GlobalConfigEvent(GlobalConfig Config, ConfigDataAction Action) : ConfigDataEvent(Action);

public sealed record SpecificConfigEvent(SpecificConfig Config, ConfigDataAction Action) : ConfigDataEvent(Action);

public sealed record ConditionUpdateEvent(SpecificConfig Config, Condition Condition, string Id, ConfigDataAction Action) : ConfigDataEvent(Action);

public sealed record HostUpdatedEvent(string Name, ActorPath Target) : IConfigEvent;