using System;
using System.Collections.ObjectModel;
using DynamicData;
using SimpleProjectManager.Shared.Services.Devices;
using Vogen;

namespace SimpleProjectManager.Client.Shared.ViewModels.Devices;

public sealed class LogGroup : IDisposable
{
    private readonly IDisposable _disposable;

    public string Key { get; }
    
    public ReadOnlyObservableCollection<LogData> Logs { get; }
    
    public LogGroup(IGroup<LogData, DateTime, string> group)
    {
        _disposable = group.Cache.Connect()
           .Bind(out var list)
           .Subscribe();
        Logs = list;
        Key = group.Key;
    }

    public void Dispose()
        => _disposable.Dispose();
}