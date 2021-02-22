using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public static class ReducerResult
    {
        public static ReducerResult<TData> Sucess<TData>(MutatingContext<TData> data) => new(data, null);

        public static ReducerResult<TData> Fail<TData>(IEnumerable<string> errors)
        {
            if (errors is string[] array)
                return new ReducerResult<TData>(null, array);

            return new ReducerResult<TData>(null, errors.ToArray());
        }

        public static ReducerResult<TData> Fail<TData>(string error)
        {
            return Fail<TData>(new[] {error});
        }
    }

    public sealed class ErrorResult : IReducerResult
    {
        public ErrorResult(Exception e)
        {
            Errors = new[] {e.Message};
        }

        public bool IsOk => false;

        public string[]? Errors { get; }
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