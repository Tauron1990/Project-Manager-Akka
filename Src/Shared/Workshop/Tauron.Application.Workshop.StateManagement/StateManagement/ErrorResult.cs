namespace Tauron.Application.Workshop.StateManagement;

public sealed class ErrorResult : IReducerResult
{
    public ErrorResult(Exception e)
    {
        Errors = new[] { e.Message };
    }

    public bool IsOk => false;

    public string[]? Errors { get; }
}