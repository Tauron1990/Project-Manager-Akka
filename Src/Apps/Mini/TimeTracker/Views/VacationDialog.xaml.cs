﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Tauron.Application;
using Tauron.Application.CommonUI.Commands;
using Tauron.Application.CommonUI.Dialogs;
using Tauron.Application.Wpf.Dialogs;
using TimeTracker.Managers;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaktionslogik für VacationDialog.xaml
    /// </summary>
    public partial class VacationDialog : IBaseDialog<DateTime[]?, DateTime>
    {
        public VacationDialog()
        {
            InitializeComponent();
        }

        public Task<DateTime[]?> Init(DateTime initalData) 
            => this.MakeObsTask<DateTime[]?>(o => new VacationDialogModel(o, initalData, Calendar.SelectedDates));
    }

    public sealed class VacationDialogModel : ObservableObject, IDisposable
    {

        public SimpleReactiveCommand Ok { get; }

        public SimpleReactiveCommand Cancel { get; }

        public VacationDialogModel(IObserver<DateTime[]?> finish, DateTime currentMounth, SelectedDatesCollection datesCollection)
        {
            Cancel = new SimpleReactiveCommand().Finish(o => o.Select(_ => default(DateTime[])).Subscribe(finish));

            var changes = Observable.Create<Unit>(o =>
                                            {
                                                void Next(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) 
                                                    => o.OnNext(Unit.Default);


                                                NotifyCollectionChangedEventHandler eventHandler = Next;
                                                datesCollection.CollectionChanged += eventHandler;
                                                return Disposable.Create((eventHandler, datesCollection), h => h.datesCollection.CollectionChanged -= h.eventHandler);
                                            });

            Ok = new SimpleReactiveCommand(from _ in changes
                                           select datesCollection.Where(SystemClock.IsWeekDay).Any(d => d.Month >= currentMounth.Month))
               .Finish(o => (from _ in o
                             select datesCollection.Where(d => d.Month >= currentMounth.Month)
                                                   .ToArray())
                          .Subscribe(finish));
        }

        public void Dispose()
        {
            Ok.Dispose();
            Cancel.Dispose();
        }
    }
}