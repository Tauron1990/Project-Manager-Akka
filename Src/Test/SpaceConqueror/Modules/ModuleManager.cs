using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using Spectre.Console;

namespace SpaceConqueror.Modules;

public sealed class ModuleManager
{
    public AssemblyLoadContext LoadContext { get; }
    
    public ImmutableDictionary<string, GamePackage> Packages { get; private set; } = ImmutableDictionary<string, GamePackage>.Empty;

    public ModuleManager()
        => LoadContext = AssemblyLoadContext.Default;

    public async ValueTask LoadModules(string path, ModuleConfiguration configuration, Action<string> messeages, Action<Exception> errors)
    {
        messeages($"Lade Haupt Spiel");
        messeages(string.Empty);
        
        GamePackage? mainAsselbly = await LoadAssembly(typeof(GameManager).Assembly, configuration, messeages);
        if(mainAsselbly is null)
        {
            AnsiConsole.Ask("Fataler Fehler beim Laden", string.Empty);
            Environment.FailFast("Fehler beim Laden der Spiele Daten");
            return;
        }
        
        Packages = Packages.Add(mainAsselbly.Name, mainAsselbly);

        if(!Directory.Exists(path)) return;
        
        foreach (string file in Directory.EnumerateFiles(path, "*.dll"))
        {
            try
            {
                messeages($"Lade Datei: {Path.GetFileName(file)}");
                messeages(string.Empty);

                Assembly asm = LoadContext.LoadFromAssemblyPath(file);
                GamePackage? gamePackage = await LoadAssembly(asm, configuration, messeages);
                
                if(gamePackage is null) continue;

                Packages = Packages.Add(gamePackage.Name, gamePackage);
            }
            catch (Exception ex)
            {
                errors(ex);
                messeages(string.Empty);
            }
        }
    }

    private async ValueTask<GamePackage?> LoadAssembly(Assembly assembly, ModuleConfiguration configuration, Action<string> messeages)
    {
        var moduleTypes = assembly.GetExportedTypes().Where(t => t.IsAssignableTo(typeof(IModule)) && t != typeof(IModule)).ToArray();

        if(moduleTypes.Length == 0) return null;
        
        var modules = ImmutableList<RegistratedModule>.Empty;
        var modName = string.Empty;

        foreach (IModule? gameModule in moduleTypes.Where(t => !t.IsAbstract).Select(Activator.CreateInstance).Cast<IModule>())
        {
            messeages($"Lade Mod Module: {gameModule.ModName} -- {gameModule.Name}");
            messeages($"Version: {gameModule.Version}");
            messeages(string.Empty);
            
            modName = gameModule.ModName;
            await gameModule.Initialize(configuration, messeages);
            modules = modules.Add(new RegistratedModule(gameModule.Name, gameModule.Version));
        }

        return new GamePackage(modName, modules);
    }
}