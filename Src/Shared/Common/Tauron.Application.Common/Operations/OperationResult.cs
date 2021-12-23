using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tauron.Operations;

public readonly record struct Error(string? Info, string Code)
{
    public Error(Exception ex)
        : this(ex.Message, ex.HResult.ToString()) { }

    public static implicit operator Error(string code)
        => new(null, code);

    public Exception CreateException()
        => new InvalidOperationException(Info ?? Code);
}

[PublicAPI]
public interface IOperationResult
{
    bool Ok { get; }
    Error[]? Errors { get; }
    object? Outcome { get; }
    string? Error { get; }
}

[PublicAPI]
public sealed record OperationResult(bool Ok, Error[]? Errors, object? Outcome) : IOperationResult
{
    [JsonIgnore] public string? Error => Errors is null ? null : string.Join(", ", Errors.Select(error => error.Info ?? error.Code));

    public static IOperationResult Success(object? result = null) => new OperationResult(Ok: true, Errors: null, result);

    public static IOperationResult Failure(in Error error, object? outcome = null)
    {
        return new OperationResult(Ok: false, new[] { error }, outcome);
    }

    public static IOperationResult Failure(IEnumerable<Error> errors, object? outcome = null)
        => new OperationResult(Ok: false, errors.ToArray(), outcome);

    public static IOperationResult Failure(params Error[] errors) => new OperationResult(Ok: false, errors, null);

    public static IOperationResult Failure(Exception error, object? outcome = null)
        => new OperationResult(Ok: false, new[] { new Error(error) }, outcome);
}