namespace Tauron.Application.Workshop.StateManagement;

public interface IReducerResult
{
    bool IsOk { get; }
    string[]? Errors { get; }
}