﻿using Akkatecture.ValueObjects;
using JetBrains.Annotations;

namespace SimpleProjectManager.Shared;

public sealed class FileName : SingleValueObject<string>
{
    public FileName(string value) : base(value) { }
    
    
    public static readonly FileName Empty = new(string.Empty);
}