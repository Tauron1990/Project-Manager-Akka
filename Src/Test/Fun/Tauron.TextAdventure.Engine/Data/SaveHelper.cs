namespace Tauron.TextAdventure.Engine.Data;

public static class SaveHelper
{
    public static IEnumerable<string> GetSaveGames()
    {
        string dic = Path.GetFullPath(Path.Combine(GameHost.RootDic, "SaveGames"));
        if(!Directory.Exists(dic))
            Directory.CreateDirectory(dic);

        return 
            from file in Directory.EnumerateFiles(dic, "*.sav")
            let name = Path.GetFileNameWithoutExtension(file)
            where !string.IsNullOrWhiteSpace(name)
            select name;
    }
}