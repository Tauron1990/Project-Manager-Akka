using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;

namespace Tauron.Localization;

[PublicAPI]
public sealed record JsonConfiguration(IDirectory RootDic, JsonFileNameMode NameMode = JsonFileNameMode.Name, string Fallback = "en")
{
    public static JsonConfiguration CreateFromApplicationPath(
        VirtualFileFactory factory,
        string langDirectory = "lang", JsonFileNameMode nameMode = JsonFileNameMode.Name, string fallback = "en")
    {
        string rootPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory, langDirectory);

        return new JsonConfiguration(factory.Local(rootPath), nameMode, fallback);
    }
}