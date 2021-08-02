using System.Collections.Immutable;

namespace ServiceHost.Client.Shared.ConfigurationServer.Data
{
    public sealed record SpecificConfig(string Id, string ConfigContent, string Info, ImmutableList<Condition> Conditions) : ConfigElement(ConfigContent, Info);

    public sealed record Condition(string Name, bool Excluding, string? AppName, ConditionType Type, ImmutableList<Condition>? Conditions, int Order);

    public enum ConditionType
    {
        And, Or,
        InstalledApp,
        DefinedApp
    }
}