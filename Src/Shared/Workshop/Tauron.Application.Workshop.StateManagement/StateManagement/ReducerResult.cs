using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement;

[PublicAPI]
public static class ReducerResult
{
    public static ReducerResult<TData> Sucess<TData>(MutatingContext<TData> data) => new(data, Errors: null);

    public static ReducerResult<TData> Fail<TData>(IEnumerable<string> errors)
    {
        if(errors is string[] array)
            return new ReducerResult<TData>(Data: null, array);

        return new ReducerResult<TData>(Data: null, errors.ToArray());
    }

    public static ReducerResult<TData> Fail<TData>(string error)
    {
        return Fail<TData>(new[] { error });
    }
}

public sealed record ReducerResult<TData>(MutatingContext<TData>? Data, string[]? Errors) : IReducerResult
{
    internal bool StartLine { get; init; }

    public bool IsOk => Errors is null;
}