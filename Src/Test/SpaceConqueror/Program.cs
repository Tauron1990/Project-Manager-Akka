using Heretic.InteractiveFiction.GamePlay;
using Heretic.InteractiveFiction.Objects;
using Heretic.InteractiveFiction.Subsystems;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpaceConqueror.Core.Console;

namespace SpaceConqueror;

public static class Program
{
    public static void Main()
    {
        IHost host = Host.CreateDefaultBuilder()
           .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<IPrintingSubsystem, ConsolePrintingSubsystem>();
                    services.AddSingleton<IGamePrerequisitesAssembler, GamePrerequisitesAssembler>();
                    services.AddSingleton<IResourceProvider, ResourceProvider>();
                    services.AddSingleton<GameLoop>();
                    services.AddSingleton<InputProcessor>();
                    services.AddSingleton<EventProvider>();
                    services.AddSingleton<Universe>();
                }).Build();

        var gameLoop = ActivatorUtilities.CreateInstance<GameLoop>(host.Services, Console.BufferWidth);
        gameLoop.Run();
    }
}