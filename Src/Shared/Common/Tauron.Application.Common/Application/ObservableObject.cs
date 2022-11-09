using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FastExpressionCompiler;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Tauron.Application;

[PublicAPI]
public static class ObservablePropertyChangedExtensions
{
    public static IObservable<TProp> WhenAny<TProp>(this IObservablePropertyChanged @this, Expression<Func<TProp>> prop)
    {
        string name = Reflex.PropertyName(prop);
        var func = prop.CompileFast();

        return @this.PropertyChangedObservable.Where(propName => propName == name).Select(_ => func());
    }
}

[PublicAPI]
public interface IObservablePropertyChanged
{
    IObservable<string> PropertyChangedObservable { get; }
}

[Serializable]
[PublicAPI]
[DebuggerStepThrough]
public abstract class ObservableObject : INotifyPropertyChangedMethod, IObservablePropertyChanged
{
    private readonly Subject<string> _propertyChnaged = new();

    [field: NonSerialized]
    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    public virtual void OnPropertyChanged([CallerMemberName] string? eventArgs = null)
        => OnPropertyChanged(new PropertyChangedEventArgs(eventArgs ?? string.Empty));

    public IObservable<string> PropertyChangedObservable => _propertyChnaged.AsObservable();

    public void SetProperty<TType>(ref TType property, TType value, [CallerMemberName] string? name = null)
    {
        if(EqualityComparer<TType>.Default.Equals(property, value)) return;

        property = value;
        OnPropertyChangedExplicit(name ?? string.Empty);
    }

    public void SetProperty<TType>(ref TType property, TType value, Action changed, [CallerMemberName] string? name = null)
    {
        if(EqualityComparer<TType>.Default.Equals(property, value)) return;

        property = value;
        OnPropertyChangedExplicit(name ?? string.Empty);
        changed();
    }

    public void SetProperty<TType>(ref TType property, TType value, Func<Task> changed, [CallerMemberName] string? name = null)
    {
        ExecutionContext? context = ExecutionContext.Capture();

        async Task RunChangedAsync()
        {
            try
            {
                await changed();
            }
            catch (Exception exception)
            {
                TauronEnviroment.GetLogger(GetType()).LogError(exception, "Error on Execute Async property Changed");
                if(context != null)
                    ExecutionContext.Run(context, state => throw (Exception)state!, exception);
            }
        }

        SetProperty(ref property, value, () => RunChangedAsync().Ignore(), name);
    }

    public virtual void OnPropertyChangedExplicit(string propertyName)
        => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

    #pragma warning disable AV1551
    public virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        => OnPropertyChanged(this, eventArgs);

    protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
    {
        if(!string.IsNullOrWhiteSpace(eventArgs.PropertyName))
            _propertyChnaged.OnNext(eventArgs.PropertyName);
        PropertyChanged?.Invoke(sender, eventArgs);
    }


    public virtual void OnPropertyChanged<T>(Expression<Func<T>> eventArgs)
        => OnPropertyChanged(new PropertyChangedEventArgs(Reflex.PropertyName(eventArgs)));
    #pragma warning restore AV1551
}