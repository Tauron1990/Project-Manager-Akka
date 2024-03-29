﻿using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public sealed record CriticalError(ErrorId Id, DateTime Occurrence, PropertyValue ApplicationPart, SimpleMessage Message, StackTraceData? StackTrace, ImmutableList<ErrorProperty> ContextData)
{
    public static readonly CriticalError Empty = new(ErrorId.New, DateTime.MinValue, PropertyValue.Empty, SimpleMessage.Empty, null, ImmutableList<ErrorProperty>.Empty);
}