using System;
using System.Runtime.Serialization;

namespace Tauron;

[Serializable]
public class OperationAlreadyStartedException : Exception
{
    public OperationAlreadyStartedException() { }

    protected OperationAlreadyStartedException(
        SerializationInfo info,
        StreamingContext context)
        : base(info, context) { }
}