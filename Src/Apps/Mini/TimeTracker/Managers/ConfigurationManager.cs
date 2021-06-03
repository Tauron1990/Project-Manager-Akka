using System.ComponentModel;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using TimeTracker.Data;

namespace TimeTracker.Managers
{
    public sealed class ConfigurationManager : INotifyPropertyChanged
    {


        public ConfigurationManager(ISubject<ProfileData> source, ConcurancyManager manager)
        {
            
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private sealed class PropertyConnector<TPropertyType>
        {
            
        }
    }
}