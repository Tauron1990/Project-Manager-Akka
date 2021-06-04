using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using FastExpressionCompiler;
using JetBrains.Annotations;
using Tauron;
using TimeTracker.Data;

namespace TimeTracker.Managers
{
    public sealed class ConfigurationManager : INotifyPropertyChanged, IDisposable
    {
        private readonly IDisposable _ceanup;

        private readonly PropertyConnector<int> _hoursAll;
        public int MonthHours
        {
            get => _hoursAll.Value;
            set => _hoursAll.Value = value;
        }

        private readonly PropertyConnector<int> _minusShortTimeHours;
        public int MinusShortTimeHours
        {
            get => _minusShortTimeHours.Value;
            set => _minusShortTimeHours.Value = value;
        }

        private readonly PropertyConnector<ImmutableList<HourMultiplicator>> _multiplicators;
        public ImmutableList<HourMultiplicator> Multiplicators
        {
            get => _multiplicators.Value ?? ImmutableList<HourMultiplicator>.Empty;
            set => _multiplicators.Value = value;
        }

        public ConfigurationManager(ISubject<ProfileData> source, ConcurancyManager manager, Action<Exception> reportError)
        {
            _hoursAll = PropertyConnector<int>.Create(source, manager, this, reportError,
                () => MonthHours, pd => pd.MonthHours, (pd, value) => pd with {MonthHours = value});

            _minusShortTimeHours = PropertyConnector<int>.Create(source, manager, this, reportError,
                () => MinusShortTimeHours, pd => pd.MinusShortTimeHours, (pd, value) => pd with {MinusShortTimeHours = value});

            _multiplicators = PropertyConnector<ImmutableList<HourMultiplicator>>.Create(source, manager, this, reportError,
                () => Multiplicators, pd => pd.Multiplicators, (pd, value) => pd with {Multiplicators = value ?? ImmutableList<HourMultiplicator>.Empty});

            _ceanup = Disposable.Create(() =>
                                        {
                                            _hoursAll.Dispose();
                                            _minusShortTimeHours.Dispose();
                                            _multiplicators.Dispose();
                                        });
        }

        void IDisposable.Dispose() => _ceanup.Dispose();

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private sealed class PropertyConnector<TPropertyType> : IDisposable
        {
            private readonly IDisposable _cleanUp;
            private readonly Action<TPropertyType?> _write;
            private TPropertyType? _value;

            public TPropertyType? Value
            {
                get => _value;
                set
                {
                    _value = value;
                    _write(value);
                }
            }

            private PropertyConnector(IObservable<TPropertyType> read, Action<TPropertyType?> write, Func<Action<TPropertyType>, IDisposable> updateValue)
            {
                _write = write;

                read.FirstOrDefaultAsync().Subscribe(v => _value = v);
                _cleanUp = updateValue(value => _value = value);
            }

            public static PropertyConnector<TPropertyType> Create(ISubject<ProfileData> data, ConcurancyManager manager, ConfigurationManager configuration, Action<Exception> reportError,
                Expression<Func<TPropertyType>> property, Func<ProfileData, TPropertyType> read, Func<ProfileData, TPropertyType?, ProfileData> write)
            {
                string name = Reflex.PropertyName(property);
                var getter = property.CompileFast();
                var equalityComparer = EqualityComparer<TPropertyType>.Default;

                var readObs = data.Select(read);

                void WriteAction(TPropertyType? value)
                {
                    if (value == null) return;

                    (from profileData in data.Take(1).SyncCall(manager) 
                     let newData = write(profileData, value) 
                     select newData)
                       .AutoSubscribe(data, reportError);

                    configuration.OnPropertyChanged(name);
                }

                IDisposable UpdateValue(Action<TPropertyType> action)
                    => (from newData in data
                        let propertyValue = getter()
                        let newValue = read(newData)
                        where !equalityComparer.Equals(newValue, propertyValue)
                        select newValue)
                       .AutoSubscribe(v =>
                                      {
                                          action(v);
                                          configuration.OnPropertyChanged(name);
                                      }, reportError);

                return new PropertyConnector<TPropertyType>(readObs, WriteAction, UpdateValue);
            }

            public void Dispose() => _cleanUp.Dispose();
        }
    }
}