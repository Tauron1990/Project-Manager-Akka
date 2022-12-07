using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Tauron.TextAdventure.Engine.Core;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class Translator : PackageElement
{
    private readonly string _fromDic;
    
    public Translator(string fromDic)
        => _fromDic = Path.Combine(Path.GetFullPath(fromDic), "lang");

    internal override void Apply(IServiceCollection serviceCollection) { }

    internal override void PostConfig(IServiceProvider serviceProvider)
    {
        var manager = serviceProvider.GetRequiredService<AssetManager>();
        string culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower(CultureInfo.InvariantCulture);

        string[] files = Directory.GetFiles(_fromDic, "*.json");

        string? targetFile = files.FirstOrDefault(d => d.EndsWith(culture, StringComparison.Ordinal));
        if(string.IsNullOrWhiteSpace(targetFile))
            targetFile = files.FirstOrDefault(d => d.EndsWith("en", StringComparison.Ordinal));
        if(string.IsNullOrWhiteSpace(targetFile))
            targetFile = files.FirstOrDefault();
        if(string.IsNullOrWhiteSpace(targetFile))
            return;

        ReadLoacels(manager, targetFile);
    }

    private void ReadLoacels(AssetManager manager, string targetFile)
    {
        using FileStream stream = File.OpenRead(targetFile);
        using JsonDocument doc = JsonDocument.Parse(stream);

        foreach (JsonProperty jsonProperty in doc.RootElement.EnumerateObject())
            manager.Add(jsonProperty.Name, () => jsonProperty.Value.ToString());
    }
}