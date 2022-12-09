using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Core;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

public sealed class Translator : PackageElement
{
    private readonly IHostEnvironment _environment;
    private readonly string _fromDic;
    
    public Translator(string fromDic, IHostEnvironment environment)
    {
        _environment = environment;
        _fromDic = Path.Combine(fromDic, "lang");
    }

    internal override void Apply(IServiceCollection serviceCollection) { }

    internal override void PostConfig(IServiceProvider serviceProvider)
    {
        var manager = serviceProvider.GetRequiredService<AssetManager>();
        var culture = $"{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower(CultureInfo.InvariantCulture)}.json";

        IDirectoryContents fromDic = _environment.ResolvePath(_fromDic);

        var files = fromDic.Where(fi => fi.Name.EndsWith(".json", StringComparison.Ordinal)).ToArray();

        IFileInfo? targetFile = files.FirstOrDefault(d => d.Name.EndsWith(culture, StringComparison.Ordinal));
        // ReSharper disable ConvertIfStatementToNullCoalescingExpression
        if(targetFile is null)
            targetFile = files.FirstOrDefault(d => d.Name.EndsWith("en.json", StringComparison.Ordinal));
        if(targetFile is null)
            targetFile = files.FirstOrDefault();
        if(targetFile is null)
            return;
        // ReSharper restore ConvertIfStatementToNullCoalescingExpression

        ReadLoacels(manager, targetFile);
    }

    private void ReadLoacels(AssetManager manager, IFileInfo targetFile)
    {
        using Stream stream = targetFile.CreateReadStream();
        using JsonDocument doc = JsonDocument.Parse(stream);

        foreach (JsonProperty jsonProperty in doc.RootElement.EnumerateObject())
        {
            var value = jsonProperty.Value.ToString();
            manager.Add(jsonProperty.Name, () => value);
        }
    }
}