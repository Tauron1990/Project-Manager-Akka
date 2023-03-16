using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace Tauron.Application;

[PublicAPI]
public static class ResourceManagerProvider
{
    private static readonly Dictionary<Assembly, ResourceManager> Resources = new();

    public static void Register(ResourceManager manager, Assembly key)
        => Resources[key] = manager;

    public static void Remove(Assembly key)
        => Resources.Remove(key);

    public static Option<string> FindResource(string name, Assembly? key, CultureInfo? cultureInfo = null)
        => FindResourceImpl(name, key, key is null, cultureInfo);

    public static Option<string> FindResource(string name, CultureInfo? cultureInfo = null)
        => FindResource(name, null, cultureInfo);

    private static Option<string> FindResourceImpl(string name, Assembly? key, bool searchEverywere = true, CultureInfo? cultureInfo = null)
    {
        if(key != null && Resources.TryGetValue(key, out ResourceManager? rm))
            return rm.GetString(name, cultureInfo).OptionNotNull();

        return searchEverywere
            ? Resources.Select(rm2 => rm2.Value.GetString(name, cultureInfo))
               .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)).OptionNotNull()
            : Option<string>.None;
    }
}