using System.Collections.Immutable;

namespace ServiceManager.Client.ViewModels.Identity
{
    public sealed record UserData(string Name, ImmutableDictionary<string, string> Claims, bool IsAnonymos);
}