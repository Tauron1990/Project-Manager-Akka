using Spectre.Console;
using Spectre.Console.Cli;

namespace ClientRunnerApp;

public static class Programm
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        AnsiConsole.WriteLine("Starting...");

        var app = new CommandApp();

        app.SetDefaultCommand<ClientApp>();
        app.Configure(c => c.SetApplicationName("Operation Client"));

        await app.RunAsync(args).ConfigureAwait(false);

        /*try
        {
            IHost host = await new ClientRunner(new ConsoleInteraction()).CreateClient(TODO).ConfigureAwait(false);
            await host.RunAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Start Abgebrochen");
            Console.ReadKey(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Schwerer Fehler:");
            Console.WriteLine(ex);
            #pragma warning disable GU0011
            Console.ReadKey(true);
        }*/
    }
}