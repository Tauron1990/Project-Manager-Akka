﻿using Stl.IO;

namespace SimpleProjectManager.Operation.Client.Config;

public record EditorData(bool Active, FilePath Path)
{
    public EditorData()
        : this(Active: false, FilePath.Empty) { }
}