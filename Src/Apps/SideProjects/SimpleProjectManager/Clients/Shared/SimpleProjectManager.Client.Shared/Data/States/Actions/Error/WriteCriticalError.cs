using System;
using System.Collections.Immutable;
using SimpleProjectManager.Shared;
using SimpleProjectManager.Shared.Services;

namespace SimpleProjectManager.Client.Shared.Data.States.Actions;

public sealed record WriteCriticalError(DateTime Occurrence, PropertyValue ApplicationPart, SimpleMessage Message, StackTraceData? StackTrace, ImmutableList<ErrorProperty> ContextData)
{
    public CriticalError ToCriticalError() => new(ErrorId.New, Occurrence, ApplicationPart, Message, StackTrace, ContextData);
}