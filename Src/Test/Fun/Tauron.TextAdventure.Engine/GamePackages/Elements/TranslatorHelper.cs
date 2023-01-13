using System.Globalization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Tauron.TextAdventure.Engine.GamePackages.Elements;

internal static class TranslatorHelper
{
    internal static IEnumerable<IFileInfo> TryFingLangFiles(IHostEnvironment provider, string baseDic, CultureInfo culture)
    {
        IDirectoryContents? contents = TryGet(provider, baseDic, culture);

        if(contents is null) yield break;

        foreach (IFileInfo file in contents)
        {
            if(file.IsDirectory) continue;

            if(file.Name.EndsWith(".json", StringComparison.Ordinal))
                yield return file;

            if(file.Name.EndsWith(".ct", StringComparison.Ordinal))
                yield return file;
        }
    }

    private static IDirectoryContents? TryGet(IHostEnvironment provider, string baseDic, CultureInfo culture)
    {
        const string langDic = "lang";
        const string fallback = "en";
        string target = culture.TwoLetterISOLanguageName.ToLower(culture);

        string searchPath = Path.Combine(baseDic, langDic, target);
        IDirectoryContents? possibleContent = provider.ResolvePath(searchPath);

        if(possibleContent.Exists && possibleContent.Any()) return possibleContent;

        searchPath = Path.Combine(baseDic, langDic, fallback);
        possibleContent = provider.ResolvePath(searchPath);

        if(possibleContent.Exists && possibleContent.Any()) return possibleContent;

        searchPath = Path.Combine(baseDic, langDic);
        possibleContent = provider.ResolvePath(searchPath);
        if(possibleContent.Exists)
            possibleContent = possibleContent
               .Where(f => f.IsDirectory && !string.IsNullOrWhiteSpace(f.PhysicalPath))
               .Select(f => provider.ResolvePath(f.PhysicalPath!))
               .FirstOrDefault();

        return possibleContent;
    }
}