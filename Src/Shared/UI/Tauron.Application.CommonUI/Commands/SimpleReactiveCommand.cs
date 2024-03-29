﻿using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;

namespace Tauron.Application.CommonUI.Commands;

[PublicAPI]
public sealed class SimpleReactiveCommand : CommandBase, IObservable<Unit>, IDisposable
{
    private readonly BehaviorSubject<bool> _canExecute = new(value: false);
    private readonly CompositeDisposable _disposable = new();
    private readonly Subject<Unit> _execute = new();

    public SimpleReactiveCommand(IObservable<bool> canExecute)
    {
        _disposable.Add(_canExecute);
        _disposable.Add(_execute);

        _disposable.Add(canExecute.Subscribe(b => _canExecute.OnNext(b), _ => _canExecute.OnNext(value: false)));
        _disposable.Add(_canExecute.Subscribe(_ => RaiseCanExecuteChanged()));
    }

    public SimpleReactiveCommand()
        : this(Observable.Return(value: true)) { }

    public void Dispose() => _disposable.Dispose();

    public IDisposable Subscribe(IObserver<Unit> observer) => _execute.Subscribe(observer);

    public override void Execute(object? parameter) => _execute.OnNext(Unit.Default);

    public override bool CanExecute(object? parameter) => _canExecute.Value;

    public SimpleReactiveCommand Finish(Func<IObservable<Unit>, IDisposable> config)
    {
        _disposable.Add(config(_execute));

        return this;
    }
}