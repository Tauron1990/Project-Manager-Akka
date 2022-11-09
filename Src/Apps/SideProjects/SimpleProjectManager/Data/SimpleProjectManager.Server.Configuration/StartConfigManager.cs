﻿using System.Collections.Immutable;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;
using SimpleProjectManager.Server.Configuration.ConfigurationExtensions;
using SimpleProjectManager.Server.Configuration.Core;
using Tauron;
using Tauron.AkkaHost;

namespace SimpleProjectManager.Server.Configuration;

[PublicAPI]
public sealed class StartConfigManager
{
    private ImmutableDictionary<string, string> _data = ImmutableDictionary<string, string>.Empty;

    private ImmutableArray<IConfigExtension> _extensions = ImmutableArray<IConfigExtension>.Empty
       .Add(new HostValueProcessor())
       .Add(new IpConfig())
       .Add(new AkkaConfig())
       .Add(new ProjectionStartConfig());

    public static StartConfigManager ConfigManager { get; } = new();

    public void RegisterExtension(IConfigExtension extension)
        => _extensions = _extensions.Add(extension);

    public void Init(string? file = null)
    {
        if(string.IsNullOrEmpty(file))
            file = "StartConfig.json";
        var dic = ReadSettings(file);

        if(dic.IsEmpty) return;

        _data = ProcessPropertys(dic);
    }

    public void ConfigurateWeb(IWebHostBuilder webHostBuilder)
        => _extensions.Foreach(e => e.Apply(_data, webHostBuilder));

    public void ConfigurateApp(IActorApplicationBuilder builder)
        => _extensions.Foreach(e => e.Apply(_data, builder));

    private ImmutableDictionary<string, string> ProcessPropertys(ImmutableDictionary<string, string> dic)
    {
        foreach (var entry in dic)
            // ReSharper disable once AccessToModifiedClosure
            dic = _extensions.Aggregate(dic, (currentValue, extension) => extension.ProcessValue(currentValue, entry.Key, entry.Value));

        return ValueReplacer.ExpandPropertys(dic);
    }

    // ReSharper disable once CognitiveComplexity
    private static ImmutableDictionary<string, string> ReadSettings(string file)
    {
        if(!File.Exists(file))
            return ImmutableDictionary<string, string>.Empty;

        JToken token = JToken.Parse(File.ReadAllText(file));
        string currentSwitch = token.Value<string>("CurrentSwitch") ?? string.Empty;

        if(string.IsNullOrEmpty(currentSwitch)) return ImmutableDictionary<string, string>.Empty;

        var dic = ImmutableDictionary<string, string>.Empty;
        JToken? toRead = token[currentSwitch];

        while (toRead is not null)
        {
            if(toRead["Settings"] is JContainer settings)
                foreach (JToken setting in settings)
                    if(setting is JProperty property)
                    {
                        var value = property.Value.Value<string>();
                        if(!dic.ContainsKey(property.Name))
                            dic = dic.Add(property.Name, value ?? string.Empty);
                    }
                    else
                    {
                        throw new InvalidOperationException("Only Propertys for Settings are Supported");
                    }

            var basedOn = toRead["BasedOn"]?.Value<string>();
            toRead = string.IsNullOrWhiteSpace(basedOn)
                ? null
                : token[basedOn];
        }

        return dic;
    }
}