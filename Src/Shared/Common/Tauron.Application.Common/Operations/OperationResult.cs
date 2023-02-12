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

    public static IOperationResult Success(object? result = null) => new OperationResult(true, null, result);

    public static IOperationResult Failure(in Error error, object? outcome = null) 
        => new OperationResult(false, new[] { error }, outcome);
    
    public static IOperationResult Failure(IErrorConvertable error, object? outcome = null) 
        => new OperationResult(false, new[] { error.ToError() }, outcome);

    public static IOperationResult Failure(IOperationResult parent, in IErrorConvertable error, object? outcome = null) 
        => new OperationResult(false, new[] { parent.Error ?? error.ToError() }, outcome);
    
    public static IOperationResult Failure(IEnumerable<Error> errors, object? outcome = null)
        => new OperationResult(false, errors.ToArray(), outcome);

    public static IOperationResult Failure(params Error[] errors) => new OperationResult(false, errors, null);

    public static IOperationResult Failure(Exception error, IFormatProvider? provider = null, object? outcome = null)
        => new OperationResult(false, new[] { Operations.Error.FromException(error, provider) }, outcome);
}