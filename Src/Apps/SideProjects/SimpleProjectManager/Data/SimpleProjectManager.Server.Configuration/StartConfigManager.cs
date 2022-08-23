using System.Collections.Immutable;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using SimpleProjectManager.Server.Configuration.ConfigurationExtensions;
using Tauron;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Configuration;

[PublicAPI]
public sealed class StartConfigManager
{
    private ImmutableArray<IConfigExtension> _extensions = ImmutableArray<IConfigExtension>.Empty;

    public static StartConfigManager ConfigManager { get; } = new();

    public void RegisterExtension(IConfigExtension extension)
        => _extensions = _extensions.Add(extension);

    public void Configurate(IActorApplicationBuilder builder, string? file = null)
    {
        if(string.IsNullOrEmpty(file))
            file = "StartConfig.json";
        var dic = ReadSettings(file);
        if(dic.IsEmpty) return;

        dic = ProcessPropertys(dic);
        _extensions.Foreach(e => e.Apply(dic, builder));
    }

    private ImmutableDictionary<string, string> ProcessPropertys(ImmutableDictionary<string, string> dic)
    {
        foreach (var entry in dic)
        {
            // ReSharper disable once AccessToModifiedClosure
            var newValue = _extensions.Aggregate(entry.Value, (currentValue, extension) => extension.ProcessValue(dic, entry.Key, currentValue));
            dic = dic.SetItem(entry.Key, newValue);
        }

        return dic;
    }

    // ReSharper disable once CognitiveComplexity
    private ImmutableDictionary<string, string> ReadSettings(string file)
    {
        if(!File.Exists(file))
            return ImmutableDictionary<string, string>.Empty;

        var token = JToken.Parse(File.ReadAllText(file));
        var currentSwitch = token.Value<string>("CurrentSwitch") ?? string.Empty;
        if(string.IsNullOrEmpty(currentSwitch)) return ImmutableDictionary<string, string>.Empty;

        var dic = ImmutableDictionary<string, string>.Empty;
        var toRead = token[currentSwitch];

        while (toRead is not null)
        {
            if(toRead["Settings"] is JContainer settings)
            {
                foreach (var setting in settings)
                {
                    if(setting is JProperty property)
                    {
                        var value = property.Value<string>();
                        if(!string.IsNullOrEmpty(value) && !dic.ContainsKey(property.Name))
                            dic = dic.Add(property.Name, value);
                    }
                    else
                        throw new InvalidOperationException("Only Propertys for Settings are Supported");
                }       
            }

            toRead = toRead["BasedOn"];
        }

        return dic;
    }
}