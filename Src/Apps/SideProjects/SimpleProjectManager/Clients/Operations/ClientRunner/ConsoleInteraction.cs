using SimpleProjectManager.Operation.Client.Core;
using SimpleProjectManager.Shared;

namespace ClientRunnerApp;

public class ConsoleInteraction : IClientInteraction
{
    public ValueTask<string> Ask(string? initial, string question)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);
                Console.WriteLine(question);

                return Console.ReadLine() ?? string.Empty;
            });

    public ValueTask<int> Ask(int? initial, string question)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);
                int result = default;

                while (true)
                {
                    Console.WriteLine(question);
                    string? consoleLine = Console.ReadLine();

                    if(string.IsNullOrWhiteSpace(consoleLine))
                        continue;

                    if(consoleLine == "q") break;

                    if(int.TryParse(consoleLine, out int number))
                    {
                        result = number;

                        break;
                    }

                    Console.WriteLine("Das ist keine Nummer");
                    Console.WriteLine();
                }

                return result;
            });

    public ValueTask<bool> Ask(bool? initial, string question)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);
                bool result = default;

                while (true)
                {
                    Console.WriteLine(question);
                    string? consoleLine = Console.ReadLine()?.ToLower();

                    if(string.IsNullOrWhiteSpace(consoleLine))
                        continue;

                    if(consoleLine == "q")
                        break;

                    if(consoleLine == bool.FalseString || consoleLine is "n" or "no" or "nein")
                    {
                        result = false;

                        break;
                    }

                    if(consoleLine == bool.TrueString || consoleLine is "y" or "yes" or "ja")
                    {
                        result = true;

                        break;
                    }

                    Console.WriteLine("Das ist keine Wahrheitswert");
                    Console.WriteLine();
                }

                return result;
            });

    public ValueTask<string> AskMultipleChoise(string? initial, string[] choises, string question)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);

                var choice = string.Empty;

                while (true)
                {
                    Console.WriteLine(question);
                    Console.WriteLine();
                    Console.WriteLine("0 = Abbrechen");
                    for (var i = 0; i < choises.Length; i++)
                        Console.WriteLine($"{i + 1} = {choises[i]}");
                    Console.WriteLine();

                    string? result = Console.ReadLine();

                    if(result == "0")
                        break;

                    if(choises.Contains(result))
                    {
                        choice = result ?? string.Empty;

                        break;
                    }

                    if(int.TryParse(result, out int indexer))
                    {
                        string? potential = choises.ElementAtOrDefault(indexer - 1);
                        if(!string.IsNullOrWhiteSpace(potential))
                        {
                            choice = potential;

                            break;
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Fehlerhafte Eingabe: {result}");
                }

                return choice;
            });

    public ValueTask<string> AskForFile(string? initial, string info)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);
                Console.WriteLine(info);
                Console.Write("Bittent Datei Eingeben:");

                return Console.ReadLine() ?? string.Empty;
            });

    private static void PrintInitial<T>(T? value)
    {
        if(value is null)
            return;

        Console.WriteLine($"Aktueller wert: {value}");
    }
}