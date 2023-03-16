using System;

namespace Tauron.Application;

[PublicAPI]
public interface IObservablePropertyChanged
{
    IObservable<string> PropertyChangedObservable { get; }
}