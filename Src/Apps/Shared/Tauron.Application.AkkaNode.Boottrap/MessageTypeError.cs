using System;
using FluentResults;

namespace Tauron.Application.AkkaNode.Bootstrap;

public sealed class MessageTypeError : Error
{
    public Type DataType { get; }

    public MessageTypeError(Type DataType)
    {
        this.DataType = DataType;
        Message = "AssemblyQualifiedName Is Empty";
    }
}