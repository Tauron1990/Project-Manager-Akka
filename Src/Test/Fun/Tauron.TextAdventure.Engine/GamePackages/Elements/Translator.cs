using System.Globalization;
using System.Text.Json;
using Cottle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Tauron.TextAdventure.Engine.Core;
using Tauron.TextAdventure.Engine.GamePackages.Core;

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

    private void PostConfig(IServiceProvider serviceProvider)
    {
        var manager = serviceProvider.GetRequiredService<AssetManager>();

        foreach (IFileInfo file in TranslatorHelper.TryFingLangFiles(_environment, _fromDic, CultureInfo.CurrentUICulture))
            ReadLoacels(manager, file);
    }

    private static void ReadLoacels(AssetManager manager, IFileInfo targetFile)
    {
        if(targetFile.Name.EndsWith(".json", StringComparison.Ordinal))
        {
            using Stream stream = targetFile.CreateReadStream();
            using JsonDocument doc = JsonDocument.Parse(stream);

            foreach (JsonProperty jsonProperty in doc.RootElement.EnumerateObject())
            {
                var value = jsonProperty.Value.ToString();
                manager.Add(jsonProperty.Name, () => value);
            }
        }
        else
        {
            manager.Add(Path.GetFileName(targetFile.Name), Create(targetFile));
        }
    }

    private static Func<IDocument> Create(IFileInfo info)
        => () =>
           {
               using Stream stream = info.CreateReadStream();

               return Document.CreateDefault(new StreamReader(stream)).DocumentOrThrow;
           };

    internal override void Load(ElementLoadContext context)
        => context.PostConfigServices.Add(PostConfig);
}