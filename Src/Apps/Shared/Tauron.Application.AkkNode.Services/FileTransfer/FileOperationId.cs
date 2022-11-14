using System;
using Vogen;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

[Instance("Empty", "")]
[ValueObject(typeof(string))]
#pragma warning disable MA0097
public readonly partial struct FileOperationId
    #pragma warning restore MA0097
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("The File Transfer id is Empty")
            : Validation.Ok;

    public static FileOperationId New()
        => From(Guid.NewGuid().ToString());
}