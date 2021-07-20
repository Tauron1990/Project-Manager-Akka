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
using Tauron.Host;

namespace Tauron.Application
{
    [PublicAPI]
    public static class ObservablePropertyChangedExtensions
    {
        public static IObservable<TProp> WhenAny<TProp>(this IObservablePropertyChanged @this, Expression<Func<TProp>> prop)
        {
            var name = Reflex.PropertyName(prop);
            var func = prop.CompileFast();

            return @this.PropertyChangedObservable.Where(s => s == name).Select(_ => func());
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

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged([CallerMemberName] string? eventArgs = null) 
            => OnPropertyChanged(new PropertyChangedEventArgs(Argument.NotNull(eventArgs!, nameof(eventArgs))));

        public IObservable<string> PropertyChangedObservable => _propertyChnaged.AsObservable();

        public void SetProperty<TType>(ref TType property, TType value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<TType>.Default.Equals(property, value)) return;

            property = value;
            OnPropertyChangedExplicit(Argument.NotNull(name!, nameof(name)));
        }

        public void SetProperty<TType>(ref TType property, TType value, Action changed, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<TType>.Default.Equals(property, value)) return;

            property = value;
            OnPropertyChangedExplicit(Argument.NotNull(name!, nameof(name)));
            changed();
        }

        public void SetProperty<TType>(ref TType property, TType value, Func<Task> changed, [CallerMemberName] string? name = null)
        {
            var context = ExecutionContext.Capture();

            SetProperty(ref property, value, new Action(() => changed().ContinueWith(t =>
                                                                                     {
                                                                                         if(t.IsCompletedSuccessfully)
                                                                                             return;
                                                                                         if(t.IsCanceled)
                                                                                             return;
                                                                                         ActorApplication.GetLogger(GetType()).LogError(t.Exception, "Error on Execute Async property Changed");
                                                                                         if(context != null)
                                                                                            ExecutionContext.Run(context, state => throw (Exception)state!, t.Exception);
                                                                                     })), name);
        }

        public virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs) 
            => OnPropertyChanged(this, Argument.NotNull(eventArgs, nameof(eventArgs)));

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (!string.IsNullOrWhiteSpace(eventArgs.PropertyName))
                _propertyChnaged.OnNext(eventArgs.PropertyName);
            PropertyChanged?.Invoke(Argument.NotNull(sender, nameof(sender)),
                Argument.NotNull(eventArgs, nameof(eventArgs)));
        }


        public virtual void OnPropertyChanged<T>(Expression<Func<T>> eventArgs) 
            => OnPropertyChanged(new PropertyChangedEventArgs(Reflex.PropertyName(Argument.NotNull(eventArgs, nameof(eventArgs)))));


        public virtual void OnPropertyChangedExplicit(string propertyName) 
            => OnPropertyChanged(new PropertyChangedEventArgs(Argument.NotNull(propertyName, nameof(propertyName))));
    }
}