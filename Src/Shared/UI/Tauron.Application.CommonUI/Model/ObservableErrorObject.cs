using System;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DynamicData;
using DynamicData.Alias;
using DynamicData.Kernel;
using JetBrains.Annotations;
using Tauron.ObservableExt;

namespace Tauron.Application.CommonUI.Model;

[PublicAPI]
public abstract class ObservableErrorObject : INotifyDataErrorInfo, IObservablePropertyChanged, INotifyPropertyChanged, IDisposable
{
    private readonly SourceCache<ValidationError, string> _errors = new(ve => ve.Property);
    private readonly SourceCache<InternalData, string> _propertys = new(d => d.Name);

    protected ObservableErrorObject()
    {
        ErrorsChanged = Observable.Create<string>(
            o => _errors.Connect().Flatten()
               .Select(c => c.Key)
               .Subscribe(o));

        Disposer.Add
        (
            _errors.Connect().Select(d => d.Property)
               .Flatten().Subscribe(c => ErrorsChangedInternal?.Invoke(this, new DataErrorsChangedEventArgs(c.Current)))
        );
    }

    protected CompositeDisposable Disposer { get; } = new();

    public int ErrorCount => _errors.Count;

    public string InternalError
    {
        get => GetValue(string.Empty);
        set => SetValue(value);
    }

    public IObservable<string> ErrorsChanged { get; }

    public void Dispose()
    {
        _errors.Dispose();
        _propertys.Dispose();
        Disposer.Dispose();
    }

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
    {
        if(string.IsNullOrWhiteSpace(propertyName))
        {
            foreach (ValidationError error in _errors.Items) yield return error.Error;

            yield break;
        }

        var opt = _errors.Lookup(propertyName);

        if(!opt.HasValue) yield break;

        yield return opt.Value.Error;
    }

    bool INotifyDataErrorInfo.HasErrors => _errors.Count != 0;

    event EventHandler<DataErrorsChangedEventArgs>? INotifyDataErrorInfo.ErrorsChanged
    {
        add => ErrorsChangedInternal += value;
        remove => ErrorsChangedInternal -= value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    IObservable<string> IObservablePropertyChanged.PropertyChangedObservable
        => _propertys.Connect().Flatten().Select(d => d.Key);


    protected void SetValue<TData>(TData value, [CallerMemberName] string? name = null)
        => _propertys.AddOrUpdate(new InternalData(value, name!));

    protected TData GetValue<TData>(TData defaultData, [CallerMemberName] string? name = null)
        #pragma warning disable EPS06
        => _propertys.Lookup(name!).ConvertOr(id => id?.Data is TData data ? data : defaultData, () => defaultData)!;
    #pragma warning restore EPS06

    protected IObserver<TData> PropertyObserver<TData>(Expression<Func<TData>> property)
    {
        string name = Reflex.PropertyName(property);

        return Observer.Create<TData>(v => SetValue(v, name), ErrorEncountered);
    }

    protected void AddValidation<TData>(Expression<Func<TData>> property, Func<IObservable<TData?>, IObservable<string>> validate)
    {
        string name = Reflex.PropertyName(property);

        Disposer.Add
        (
            _propertys.Connect()
               .Where(o => string.Equals(o.Name, name, StringComparison.Ordinal))
               .Flatten()
               .SelectMany(
                    c =>
                    {
                        static IObservable<Action> CreateEmpty()
                            => Observable.Return<Action>(() => { });

                        IObservable<Action> CreateChangeAction()
                        {
                            return validate(Observable.Return(c.Current.Data is TData data ? data : default))
                               .FirstOrDefaultAsync().ConditionalSelect()
                               .ToResult<Action>(
                                    o =>
                                    {
                                        o.When(
                                            string.IsNullOrWhiteSpace,
                                            s => s.Select<string?, Action>(_ => () => _errors.RemoveKey(name)));
                                        o.When(
                                            e => !string.IsNullOrWhiteSpace(e),
                                            no => no.NotEmpty()
                                               .Select<string, Action>(e => () => _errors.AddOrUpdate(new ValidationError(e, name))));
                                    });
                        }

                        return c.Reason switch
                        {
                            ChangeReason.Add => CreateChangeAction(),
                            ChangeReason.Update => CreateChangeAction(),
                            ChangeReason.Remove => CreateEmpty(),
                            _ => CreateEmpty(),
                        };
                    })
               .AutoSubscribe(a => a(), ErrorEncountered)
        );
    }

    public virtual void ErrorEncountered(Exception exception)
    {
        InternalError = exception.Message;
        _errors.AddOrUpdate(new ValidationError(exception.Message, "InternalError"));
    }

    private event EventHandler<DataErrorsChangedEventArgs>? ErrorsChangedInternal;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private sealed record ValidationError(string Error, string Property);

    private sealed record InternalData(object? Data, string Name);
}