using System.Collections.Immutable;
using System.Globalization;
using FluentValidation.Results;
using SimpleProjectManager.Operation.Client.Core;
using SimpleProjectManager.Shared;
using Spectre.Console;

namespace ClientRunnerApp;

public class ConsoleInteraction : IClientInteraction
{
    public ValueTask<string> Ask(string? initial, string question)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);

                return AnsiConsole.Ask<string>(question);
            });

    public ValueTask<int> Ask(int? initial, string question)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);
                int result = default;

                while (true)
                {
                    var consoleLine = AnsiConsole.Ask<string>(question);

                    if(string.IsNullOrWhiteSpace(consoleLine))
                        continue;

                    if(string.Equals(consoleLine, "q", StringComparison.CurrentCulture)) break;

                    if(int.TryParse(consoleLine, NumberStyles.Any, CultureInfo.CurrentCulture, out int number))
                    {
                        result = number;

                        break;
                    }

                    AnsiConsole.WriteLine("Das ist keine Nummer");
                    AnsiConsole.WriteLine();
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
                    string consoleLine = AnsiConsole.Ask<string>(question).ToLower(CultureInfo.CurrentUICulture);

                    if(string.IsNullOrWhiteSpace(consoleLine))
                        continue;

                    if(string.Equals(consoleLine, "q", StringComparison.CurrentCulture))
                        break;

                    if(string.Equals(consoleLine, bool.FalseString, StringComparison.CurrentCulture) || consoleLine is "n" or "no" or "nein")
                    {
                        result = false;

                        break;
                    }

                    if(string.Equals(consoleLine, bool.TrueString, StringComparison.CurrentCulture) || consoleLine is "y" or "yes" or "ja")
                    {
                        result = true;

                        break;
                    }

                    AnsiConsole.WriteLine("Das ist keine Wahrheitswert");
                    AnsiConsole.WriteLine();
                }

                return result;
            });

    public ValueTask<string> AskMultipleChoise(string? initial, string[] choises, string question)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);

                string selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                       .Title(question)
                       .AddChoices(choises));

                return selection;
            });

    public ValueTask<string> AskForFile(string? initial, string info)
        => To.VTask(
            () =>
            {
                PrintInitial(initial);
                AnsiConsole.WriteLine(info);

                return AnsiConsole.Ask<string>("Bitte Datein Eigeben:");
            });

    public bool AskForCancel(string operation, Exception error)
    {
        AnsiConsole.WriteLine($"Fehler bei der Operation: {operation}");
        AnsiConsole.WriteException(error);
        AnsiConsole.Write("Benden(y/n)?");
        ConsoleKeyInfo result = Console.ReadKey();
        AnsiConsole.WriteLine();

        return result.Key == ConsoleKey.Y;
    }

    public void Display(ImmutableList<ValidationFailure> configError)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.WriteLine("Validation Fehler");

        foreach (ValidationFailure failure in configError)
            AnsiConsole.WriteLine(failure.ErrorMessage);

        AnsiConsole.WriteLine();
    }

    private static void PrintInitial<T>(T? value)
    {
        if(value is null)
            return;

        AnsiConsole.WriteLine($"Aktueller wert: {value}");
    }
}