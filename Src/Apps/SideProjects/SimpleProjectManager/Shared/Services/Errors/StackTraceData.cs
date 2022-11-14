using System.Diagnostics;
using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared.Services;

#pragma warning disable MA0097
public sealed class StackTraceData : SingleValueObject<string>
    #pragma warning restore MA0097
{
    public StackTraceData(string value) : base(value) { }

    public StackTraceData(StackTrace stackTrace)
        : base(stackTrace.ToString()) { }

    public static StackTraceData? FromException(Exception exception)
        => string.IsNullOrWhiteSpace(exception.StackTrace) ? null : new StackTraceData(exception.StackTrace);
}