using System.Collections.Immutable;

namespace ServiceHost.Client.Shared.ConfigurationServer.Data
{
    public sealed record SpecificConfig(string Id, string ConfigContent, string Info, ImmutableList<Condition> Conditions) : ConfigElement(ConfigContent, Info);

    public abstract record Condition(string Name, bool Excluding, int Order);

    public sealed record AppCondition(string Name, bool Excluding, string AppName, int Order = 2) : Condition(Name, Excluding, Order);

    public sealed record InstalledAppCondition(string Name, bool Excluding, string AppName, int Order = 1) : Condition(Name, Excluding, Order);

    public sealed record AndCondition(string Name, bool Excluding, ImmutableList<Condition> Conditions, int Order = 3) : Condition(Name, Excluding, Order);

    public sealed record OrCondition(string Name, bool Excluding, ImmutableList<Condition> Conditions, int Order = 3) : Condition(Name, Excluding, Order);
}