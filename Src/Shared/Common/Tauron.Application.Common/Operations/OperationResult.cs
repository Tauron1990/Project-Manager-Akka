using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Tauron.Operations;

[PublicAPI]
public sealed record OperationResult(bool Ok, Error[]? Errors, object? Outcome) : IOperationResult
{
    [JsonIgnore]
    public string? Error => Errors is null ? null : string.Join(", ", Errors.Select(error => error.Info ?? error.Code));

    public static IOperationResult Success(object? result = null) => new OperationResult(Ok: true, Errors: null, result);

    public static IOperationResult Failure(in Error error, object? outcome = null)
    {
        return new OperationResult(Ok: false, new[] { error }, outcome);
    }

    public static IOperationResult Failure(IEnumerable<Error> errors, object? outcome = null)
        => new OperationResult(Ok: false, errors.ToArray(), outcome);

    public static IOperationResult Failure(params Error[] errors) => new OperationResult(Ok: false, errors, Outcome: null);

    public static IOperationResult Failure(Exception error, object? outcome = null)
        => new OperationResult(Ok: false, new[] { new Error(error) }, outcome);
}