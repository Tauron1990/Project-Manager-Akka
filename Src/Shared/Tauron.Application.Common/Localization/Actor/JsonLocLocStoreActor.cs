using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Akka.Util;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tauron.Application.VirtualFiles;

namespace Tauron.Localization.Actor;

[UsedImplicitly]
public sealed class JsonLocLocStoreActor : LocStoreActorBase
{
    private static readonly char[] Sep = { '.' };

    private readonly JsonConfiguration? _configuration;
    private readonly Dictionary<string, Dictionary<string, JToken>> _files = new();
    private bool _isInitialized;

    public JsonLocLocStoreActor(ILifetimeScope scope, VirtualFileFactory factory)
    {
        try
        {
            _configuration = scope.ResolveOptional<JsonConfiguration>()
                ?? JsonConfiguration.CreateFromApplicationPath(factory);
        }
        catch (DependencyResolutionException)
        {
            _configuration = JsonConfiguration.CreateFromApplicationPath(factory);
        }
    }

    protected override Option<object> TryQuery(string name, CultureInfo target)
    {
        if (_configuration is null)
            return Option<object>.None;

        EnsureInitialized();

        do
        {
            var obj = LookUp(name, target);

            if (obj != null)
                return obj;

            target = target.Parent;
        } while (!Equals(target, CultureInfo.InvariantCulture));

        return LookUp(name, CultureInfo.GetCultureInfo(_configuration.Fallback)).OptionNotNull();
    }

    private object? LookUp(string name, CultureInfo target)
    {
        if (_configuration == null) return null;

        string language = _configuration.NameMode switch
        {
            JsonFileNameMode.Name => target.Name,
            JsonFileNameMode.TwoLetterIsoLanguageName => target.TwoLetterISOLanguageName,
            JsonFileNameMode.ThreeLetterIsoLanguageName => target.ThreeLetterISOLanguageName,
            JsonFileNameMode.ThreeLetterWindowsLanguageName => target.ThreeLetterWindowsLanguageName,
            JsonFileNameMode.DisplayName => target.DisplayName,
            JsonFileNameMode.EnglishName => target.EnglishName,
            _ => throw new InvalidOperationException("No Valid Json File Name Mode")
        };

        if (!_files.TryGetValue(language, out var entrys) || !entrys.TryGetValue(name, out var entry) ||
            entry is not JValue value) return null;

        return value.Type == JTokenType.String ? EscapeHelper.Decode(value.Value<string>()) : value.Value;
    }

    private void EnsureInitialized()
    {
        if (_isInitialized) return;
        if (_configuration == null) return;

        _files.Clear();


        foreach (var file in from f in _configuration.RootDic.Files
                             where f.Extension == ".json"
                             select f)
        {
            //var text = File.ReadAllText(file, Encoding.UTF8);
            using var stream = file.Open(FileAccess.Read);
            var text = new StreamReader(stream).ReadToEnd();
            var name = GetName(file.OriginalPath);

            if (string.IsNullOrWhiteSpace(name)) return;

            _files[name] = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(text) ?? new Dictionary<string, JToken>();
        }

        _isInitialized = true;
    }

    private static string? GetName(string fileName)
    {
        var data = Path.GetFileNameWithoutExtension(fileName).Split(Sep, StringSplitOptions.RemoveEmptyEntries);

        return data.Length switch
        {
            2 => data[1],
            1 => data[0],
            _ => null
        };
    }
}