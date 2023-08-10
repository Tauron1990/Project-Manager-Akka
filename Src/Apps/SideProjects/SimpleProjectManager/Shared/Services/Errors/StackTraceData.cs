using System.Diagnostics;
using System.Runtime.Serialization;
using Akkatecture.ValueObjects;
using MemoryPack;

namespace SimpleProjectManager.Shared.Services;

[DataContract, MemoryPackable]
public sealed partial class StackTraceData : SingleValueObject<string>
{
    [MemoryPackConstructor]
    public StackTraceData(string value) : base(value) { }

    public StackTraceData(StackTrace stackTrace)
        : base(stackTrace.ToString()) { }

    public static StackTraceData? FromException(Exception exception)
        => string.IsNullOrWhiteSpace(exception.StackTrace) ? null : new StackTraceData(exception.StackTrace);
}