using System;
using System.Collections.ObjectModel;
using DynamicData;
using SimpleProjectManager.Shared.Services.Devices;

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
        
        Console.WriteLine("Wire Up New Log Entrys");
        group.Cache.Connect()
           .Subscribe(c => Console.WriteLine($"New Log Entrys Count: {c.Count}"));
    }

    public void Dispose()
        => _disposable.Dispose();
}