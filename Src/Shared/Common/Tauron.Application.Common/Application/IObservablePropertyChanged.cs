using System;
using JetBrains.Annotations;

namespace Tauron.Application;

[PublicAPI]
public interface IObservablePropertyChanged
{
    IObservable<string> PropertyChangedObservable { get; }
}