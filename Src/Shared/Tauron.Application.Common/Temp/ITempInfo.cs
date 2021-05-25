using System;
using Akka.Util;
using JetBrains.Annotations;

namespace Tauron.Temp
{
    [PublicAPI]
    public interface ITempInfo : IDisposable
    {
        string FullPath { get; }
        Option<ITempDic> Parent { get; }
    }
}