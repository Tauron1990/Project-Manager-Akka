using System;
using JetBrains.Annotations;

namespace Tauron;

public class ProgressEventArgs : EventArgs
{
    public ProgressEventArgs(ProgressStatistic progressStatistic)
        => ProgressStatistic = progressStatistic;

    [PublicAPI]
    public ProgressStatistic ProgressStatistic { get; }
}