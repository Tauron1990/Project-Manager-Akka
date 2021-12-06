using SimpleProjectManager.Operation.Client.Shared.Config;
using SimpleProjectManager.Shared.ServerApi;

namespace SimpleProjectManager.Operation.Client.Shared.Setup;

public sealed class SetupRunner
{
    private readonly ConfigManager _configManager = new();

    private async ValueTask RunSetup(string? ip)
    {
        if (string.IsNullOrWhiteSpace(_configManager.Configuration.ServerIp))
        {
            if (string.IsNullOrWhiteSpace(ip)) 
                ip = await FetchIp();

            await _configManager.Set(_configManager.Configuration with{ ServerIp = ip});
        }
        else if (!string.IsNullOrWhiteSpace(ip) && ip != _configManager.Configuration.ServerIp)
            await _configManager.Set(_configManager.Configuration with { ServerIp = ip });

        await AskForModes();
    }

    private async ValueTask AskForModes()
    {
        Console.WriteLine();
        Console.WriteLine("Wie ist der Name dises PCs:");
        var name = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Bitte Namen angeben");
            name = Console.ReadLine();
        }

        var imageEdit = AskForBool("Werden auf diesem PC Bilder Beabeited?");
        var device = AskForBool("Wird hier gedruckt?");

        await _configManager.Set(_configManager.Configuration with { ImageEditor = imageEdit, Device = device });
    }

    private static bool AskForBool(string initialAsk)
    {
        Console.WriteLine();
        Console.WriteLine(initialAsk);

        var name = Console.ReadLine();
        bool? result = null;
        
        do
        {
            switch (name?.ToLower())
            {
                case "y":
                case "yes":

                case "j":
                case "ja":

                case "true":
                    result = true;
                    break;

                case "n":
                case "no":
                case "nein":

                case "false":
                    result = false;
                    break;

                default:
                    Console.WriteLine("Bitte richtigen wert Angegeben:");
                    name = Console.ReadLine();

                    break;
            }
        } while (result is null);

        return result.Value;
    }

    private static async ValueTask<string> FetchIp()
    {
        Console.WriteLine("Operations Client Setup");
        Console.Write("Server Ip: ");
        var ip = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(ip))
            throw new InvalidOperationException("Keine Ip Eingegeben");

        using var client = new HttpClient();
        using var response = await client.GetAsync(ApiPaths.PingApi);
        response.EnsureSuccessStatusCode();

        if (await response.Content.ReadAsStringAsync() != "ok")
            throw new InvalidOperationException("Der Serve Ping hat nicht Funktioniert");

        return ip;
    }
    
    public static async ValueTask Run(string? ip)
    {
        var setup = new SetupRunner();

        await setup.RunSetup(ip);
    }
}