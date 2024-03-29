using System.Collections.Immutable;
using FluentValidation.Results;

namespace SimpleProjectManager.Operation.Client.Core;

public interface IClientInteraction
{
    ValueTask<string> Ask(string? initial, string question);
    ValueTask<int> Ask(int? initial, string question);

    ValueTask<bool> Ask(bool? initial, string question);

    ValueTask<string> AskMultipleChoise(string? initial, string[] choises, string question);

    ValueTask<string> AskForFile(string? initial, string info);

    bool AskForCancel(string operation, Exception error);
    void Display(ImmutableList<ValidationFailure> configError);
}