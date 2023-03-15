using System.Globalization;
using Akka.Util;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tauron.Application.VirtualFiles;
using Tauron.Operations;

namespace Tauron.Localization.Actor;

[UsedImplicitly]
public sealed class JsonLocLocStoreActor : LocStoreActorBase
{
    private static readonly char[] Sep = { '.' };

    private readonly JsonConfiguration? _configuration;
    private readonly Dictionary<string, Dictionary<string, JToken>> _files = new(StringComparer.Ordinal);
    private bool _isInitialized;

    public JsonLocLocStoreActor(IServiceProvider scope, VirtualFileFactory factory, ILogger<JsonLocLocStoreActor> logger)
    {
        try
        {
            _configuration = scope.GetService<JsonConfiguration>()
                          ?? JsonConfiguration.CreateFromApplicationPath(factory);
        }
        catch (Exception e)
        {
            _configuration = JsonConfiguration.CreateFromApplicationPath(factory);
            logger.LogError(e, "Error on get JsonConfiguration use Default");
        }
    }

    protected override TriOption<object> TryQuery(string name, CultureInfo target)
    {
        if(_configuration is null)
            return Option<object>.None;

        EnsureInitialized();

        do
        {
            var obj = LookUp(name, target);

            if(!obj.IsNone)
                return obj;

            target = target.Parent;
        } while (!Equals(target, CultureInfo.InvariantCulture));

        return LookUp(name, CultureInfo.GetCultureInfo(_configuration.Fallback));
    }

    private TriOption<object> LookUp(string name, CultureInfo target)
    {
        if(_configuration is null) return TriOption<object>.None;

        string language = _configuration.NameMode switch
        {
            JsonFileNameMode.Name => target.Name,
            JsonFileNameMode.TwoLetterIsoLanguageName => target.TwoLetterISOLanguageName,
            JsonFileNameMode.ThreeLetterIsoLanguageName => target.ThreeLetterISOLanguageName,
            JsonFileNameMode.ThreeLetterWindowsLanguageName => target.ThreeLetterWindowsLanguageName,
            JsonFileNameMode.DisplayName => target.DisplayName,
            JsonFileNameMode.EnglishName => target.EnglishName,
            _ => "err",
        };

        if(string.Equals(language, "err", StringComparison.Ordinal))
            return new InvalidOperationException("No Valid Json File Name Mode");
        
        if(!_files.TryGetValue(language, out var entrys) || !entrys.TryGetValue(name, out JToken? entry) ||
           entry is not JValue value) return TriOption<object>.None;

        object? result =  value.Type == JTokenType.String ? EscapeHelper.Decode(value.Value<string>()) : value.Value;
        
        if(result is null)
            return TriOption<object>.None;

        return result;
    }

    private void EnsureInitialized()
    {
        if(_isInitialized) return;
        if(_configuration is null) return;

        _files.Clear();


        foreach (IFile? file in from f in () => _configuration.RootDic.Files()
                                where string.Equals(f.Extension, ".json", StringComparison.Ordinal)
                                select f)
        {
            //var text = File.ReadAllText(file, Encoding.UTF8);
            using Stream stream = file.Open(FileAccess.Read);
            string text = new StreamReader(stream).ReadToEnd();
            string? name = GetName(file.OriginalPath);

            if(string.IsNullOrWhiteSpace(name)) return;

            _files[name] = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(text) ?? new Dictionary<string, JToken>(StringComparer.Ordinal);
        }

        _isInitialized = true;
    }

    private static string? GetName(string fileName)
    {
        string[] data = Path.GetFileNameWithoutExtension(fileName).Split(Sep, StringSplitOptions.RemoveEmptyEntries);

        return data.Length switch
        {
            2 => data[1],
            1 => data[0],
            _ => null,
        };
    }
}