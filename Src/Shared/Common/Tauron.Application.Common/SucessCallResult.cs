namespace Tauron;

public sealed record SucessCallResult<TResult>(TResult Result) : CallResult<TResult>(true);