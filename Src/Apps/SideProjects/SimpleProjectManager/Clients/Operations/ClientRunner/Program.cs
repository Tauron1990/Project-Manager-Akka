using Microsoft.Extensions.Hosting;
using SimpleProjectManager.Operation.Client;

namespace ClientRunnerApp;

public static class Programm
{
    
    [STAThread]
    public static async Task Main(string[] args)
    {
        try
        {
            var host = await new ClientRunner(new ConsoleInteraction()).CreateClient(args);
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Schwerer Fehler:");
            Console.WriteLine(ex);
            Console.ReadLine();
        }
    }
}