﻿using Akkatecture.ValueObjects;

namespace SimpleProjectManager.Shared;

public sealed class FileMime : SingleValueObject<string>
{
    public FileMime(string value) : base(value) { }

    public static readonly FileMime Generic = new("APPLICATION/octet-stream");
}