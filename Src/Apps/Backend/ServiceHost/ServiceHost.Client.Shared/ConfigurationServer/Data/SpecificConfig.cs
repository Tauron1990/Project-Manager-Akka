using System.Collections.Immutable;

namespace ServiceHost.Client.Shared.ConfigurationServer.Data
{
    public sealed record SpecificConfig(string Id, string ConfigContent, string Info, bool Merge, ImmutableList<Condition> Conditions) : ConfigElement(ConfigContent, Info);

    public abstract record Condition(string Name, bool Excluding);

    public sealed record AppCondition(string Name, bool Excluding, string AppName) : Condition(Name, Excluding);

    public sealed record InstalledAppCondition(string Name, bool Excluding, string AppName) : Condition(AppName, Excluding);


}