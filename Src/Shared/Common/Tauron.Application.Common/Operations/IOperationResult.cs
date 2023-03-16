namespace Tauron.Operations;

[PublicAPI]
public interface IOperationResult
{
    bool Ok { get; }
    Error[]? Errors { get; }
    object? Outcome { get; }
    string? Error { get; }
}