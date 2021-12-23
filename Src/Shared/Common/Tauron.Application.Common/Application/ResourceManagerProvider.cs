using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using Akka.Util;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public static class ResourceManagerProvider
{
    private static readonly Dictionary<Assembly, ResourceManager> Resources = new();

    public static void Register(ResourceManager manager, Assembly key)
        => Resources[key] = manager;

    public static void Remove(Assembly key)
        => Resources.Remove(key);

    public static Option<string> FindResource(string name, Assembly? key)
        => FindResourceImpl(name, key, key is null);

    public static Option<string> FindResource(string name)
        => FindResource(name, null);

    private static Option<string> FindResourceImpl(string name, Assembly? key, bool searchEverywere = true)
    {
        if (key != null && Resources.TryGetValue(key, out var rm))
            return rm.GetString(name).OptionNotNull();

        return searchEverywere
            ? Resources.Select(rm2 => rm2.Value.GetString(name))
               .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)).OptionNotNull()
            : Option<string>.None;
    }
}