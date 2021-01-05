using System;
using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    public static class ReducerResult
    {
        public static ReducerResult<TData> Sucess<TData>(MutatingContext<TData> data)
            => new(data, null);

        public static ReducerResult<TData> Fail<TData>(MutatingContext<TData> context, IEnumerable<string> errors)
        {
            if(errors is string[] array)
                return new ReducerResult<TData>(null, array);

            return new ReducerResult<TData>(null, errors.ToArray());
        }

        public static ReducerResult<TData> Fail<TData>(MutatingContext<TData> context, string error) => Fail(context, new[] { error });
    }

    public sealed class ErrorResult : IReducerResult
    {
        public bool IsOk => false;

        public string[]? Errors { get; }

        public ErrorResult(Exception e)
            => Errors = new[] { e.Message };
    }

    public interface IReducerResult
    {
        bool IsOk { get; }
        string[]? Errors { get; }
    }

    public sealed record ReducerResult<TData>(MutatingContext<TData>? Data, string[]? Errors) : IReducerResult
    {
        internal bool StartLine { get; init; }
        
        public bool IsOk => Errors == null;
    }
}