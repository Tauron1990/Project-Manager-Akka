using System;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Application.VirtualFiles;

namespace Tauron.Temp;

[PublicAPI]
public interface ITempInfo : IDisposable
{
    PathInfo FullPath { get; }
    Option<ITempDic> Parent { get; }
}