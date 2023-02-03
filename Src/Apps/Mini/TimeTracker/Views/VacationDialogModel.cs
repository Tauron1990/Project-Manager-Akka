using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using Tauron.Application;
using Tauron.Application.CommonUI.Commands;
using TimeTracker.Managers;

namespace TimeTracker.Views;

public sealed class VacationDialogModel : ObservableObject, IDisposable
{
    public VacationDialogModel(IObserver<DateTime[]?> finish, DateTime currentMounth, SelectedDatesCollection datesCollection)
    {
        Cancel = new SimpleReactiveCommand().Finish(o => o.Select(_ => default(DateTime[])).Subscribe(finish));

        var changes = Observable.Create<Unit>(
            o =>
            {
                void Next(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
                    => o.OnNext(Unit.Default);


                NotifyCollectionChangedEventHandler eventHandler = Next;
                datesCollection.CollectionChanged += eventHandler;

                return Disposable.Create((eventHandler, datesCollection), h => h.datesCollection.CollectionChanged -= h.eventHandler);
            });

        Ok = new SimpleReactiveCommand(
                from _ in changes
                select datesCollection.Where(SystemClock.IsWeekDay).Any(d => d.Month >= currentMounth.Month))
            .Finish(
                o => (from _ in o
                        select datesCollection.Where(d => d.Month >= currentMounth.Month)
                            .ToArray())
                    .Subscribe(finish));
    }

    public SimpleReactiveCommand Ok { get; }

    public SimpleReactiveCommand Cancel { get; }

    public void Dispose()
    {
        Ok.Dispose();
        Cancel.Dispose();
    }
}