// See https://aka.ms/new-console-template for more information

using SpaceConqueror;
using Spectre.Console;

Console.Title = "Space Conqueror";

await using GameManager manager = new(Path.GetFullPath("Mods"));

AnsiConsole.WriteLine();

await AnsiConsole.Status()
   .SpinnerStyle(Style.Parse("orangered1"))
   .StartAsync("Starte Spiel...", manager.Initialize);

AnsiConsole.Clear();

await manager.Run();