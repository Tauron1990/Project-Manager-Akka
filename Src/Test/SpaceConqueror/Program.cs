using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Tauron.TextAdventure.Engine;

namespace SpaceConqueror;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            IHost host = await GameHost.Create<Game>(args).ConfigureAwait(false);

            await host.RunAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            AnsiConsole.Clear();
            AnsiConsole.WriteLine("Schwerwigender Fehler");
            AnsiConsole.WriteLine();
            AnsiConsole.WriteException(e);
        }
    }
}