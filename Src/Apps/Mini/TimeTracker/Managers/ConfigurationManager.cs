﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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

        private readonly PropertyConnector<int> _dailyHours;

        private readonly PropertyConnector<int> _hoursAll;

        private readonly PropertyConnector<int> _minusShortTimeHours;

        private readonly PropertyConnector<ImmutableList<HourMultiplicator>> _multiplicators;

        public ConfigurationManager(DataManager source, Action<Exception> reportError)
        {
            _hoursAll = PropertyConnector<int>.Create(
                source,
                this,
                reportError,
                () => MonthHours,
                pd => pd.MonthHours,
                (pd, value) => pd with { MonthHours = value });

            _minusShortTimeHours = PropertyConnector<int>.Create(
                source,
                this,
                reportError,
                () => MinusShortTimeHours,
                pd => pd.MinusShortTimeHours,
                (pd, value) => pd with { MinusShortTimeHours = value });

            _multiplicators = PropertyConnector<ImmutableList<HourMultiplicator>>.Create(
                source,
                this,
                reportError,
                () => Multiplicators,
                pd => pd.Multiplicators,
                (pd, value) => pd with { Multiplicators = value ?? ImmutableList<HourMultiplicator>.Empty });

            _dailyHours = PropertyConnector<int>.Create(
                source,
                this,
                reportError,
                () => DailyHours,
                pd => pd.DailyHours,
                (pd, value) => pd with { DailyHours = value });

            _ceanup = Disposable.Create(
                (_hoursAll, _minusShortTimeHours, _multiplicators, _dailyHours),
                s =>
                {
                    var (hoursAll, minusShortTimeHours, multiplicators, dailyHours) = s;
                    hoursAll.Dispose();
                    minusShortTimeHours.Dispose();
                    multiplicators.Dispose();
                    dailyHours.Dispose();
                });
        }

        public int MonthHours
        {
            get => _hoursAll.Value;
            set => _hoursAll.Value = value;
        }

        public int MinusShortTimeHours
        {
            get => _minusShortTimeHours.Value;
            set => _minusShortTimeHours.Value = value;
        }

        private ImmutableList<HourMultiplicator> Multiplicators => _multiplicators.Value ?? ImmutableList<HourMultiplicator>.Empty;

        public int DailyHours
        {
            get => _dailyHours.Value;
            set => _dailyHours.Value = value;
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

            private PropertyConnector(IObservable<TPropertyType> read, Action<TPropertyType?> write, Func<Action<TPropertyType>, IDisposable> updateValue)
            {
                _write = write;

                read.FirstOrDefaultAsync().Subscribe(v => _value = v);
                _cleanUp = updateValue(value => _value = value);
            }

            internal TPropertyType? Value
            {
                get => _value;
                set
                {
                    _value = value;
                    _write(value);
                }
            }

            public void Dispose() => _cleanUp.Dispose();

            internal static PropertyConnector<TPropertyType> Create(
                DataManager data, ConfigurationManager configuration, Action<Exception> reportError,
                Expression<Func<TPropertyType>> property, Func<ProfileData, TPropertyType> read, Func<ProfileData, TPropertyType?, ProfileData> write)
            {
                string name = Reflex.PropertyName(property);
                var getter = property.CompileFast();
                var equalityComparer = EqualityComparer<TPropertyType>.Default;

                var readObs = data.Stream.Select(read);

                void WriteAction(TPropertyType? value)
                {
                    if (value == null) return;

                    data.Mutate(
                            o => from profileData in o
                                 let newData = write(profileData, value)
                                 select newData)
                       .AutoSubscribe(reportError);

                    configuration.OnPropertyChanged(name);
                }

                IDisposable UpdateValue(Action<TPropertyType> action)
                    => (from newData in data.Stream.Skip(1)
                        let propertyValue = getter()
                        let newValue = read(newData)
                        where !equalityComparer.Equals(newValue, propertyValue)
                        select newValue)
                       .AutoSubscribe(
                            v =>
                            {
                                action(v);
                                configuration.OnPropertyChanged(name);
                            },
                            reportError);

                return new PropertyConnector<TPropertyType>(readObs, WriteAction, UpdateValue);
            }
        }
    }
}