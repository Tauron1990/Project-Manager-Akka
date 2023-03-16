using System.ComponentModel;

namespace Tauron.Application;

/// <summary>The NotifyPropertyChangedMethod interface.</summary>
[PublicAPI]
public interface INotifyPropertyChangedMethod : INotifyPropertyChanged
{
    void OnPropertyChanged(string eventArgs);
}