using System;

namespace Tauron;

public class ProgressEventArgs : EventArgs
{
    public ProgressEventArgs(ProgressStatistic progressStatistic)
        => ProgressStatistic = progressStatistic;

    [PublicAPI]
    public ProgressStatistic ProgressStatistic { get; }
}