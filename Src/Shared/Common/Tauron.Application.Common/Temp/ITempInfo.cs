﻿using System;
using Tauron.Application.VirtualFiles;

namespace Tauron.Temp;

[PublicAPI]
public interface ITempInfo : IDisposable
{
    PathInfo FullPath { get; }
    Option<ITempDic> Parent { get; }
}