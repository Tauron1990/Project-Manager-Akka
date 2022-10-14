using System;
using Vogen;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

[Instance("Empty", "")]
[ValueObject(typeof(string))]
public readonly partial struct FileOperationId
{
    private static Validation Validate(string value)
        => string.IsNullOrWhiteSpace(value)
            ? Validation.Invalid("The File Transfer id is Empty")
            : Validation.Ok;

    public static FileOperationId New()
        => From(Guid.NewGuid().ToString());
}