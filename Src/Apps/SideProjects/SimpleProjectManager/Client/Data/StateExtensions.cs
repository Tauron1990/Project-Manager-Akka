﻿using System.Reactive.Linq;

namespace SimpleProjectManager.Client.Data;

public static class StateExtensions
{
    public static IObservable<bool> AndIsOnline(this IObservable<bool> condition, IOnlineMonitor onlineMonitor)
        => condition.CombineLatest(onlineMonitor.Online, (conditionResult, isOnline) => conditionResult && isOnline);
}