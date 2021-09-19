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

namespace Tauron.Application.CommonUI.Model
{
    [PublicAPI]
    public abstract class ObservableErrorObject : INotifyDataErrorInfo, IObservablePropertyChanged, INotifyPropertyChanged, IDisposable
    {
        private readonly SourceCache<ValidationError, string> _errors    = new(ve => ve.Property);
        private readonly SourceCache<InternalData, string>    _propertys = new(d => d.Name);
        
        protected CompositeDisposable Disposer { get; } = new();

        public int ErrorCount => _errors.Count;

        protected ObservableErrorObject()
        {
            ErrorsChanged = Observable.Create<string>(o => _errors.Connect().Flatten()
                                                                  .Select(c => c.Key)
                                                                  .Subscribe(o));

            _errors.Connect().Select(d => d.Property)
                   .Flatten().Subscribe(c => ErrorsChangedInternal?.Invoke(this, new DataErrorsChangedEventArgs(c.Current)))
                   .DisposeWith(Disposer);
        }
        

        protected void SetValue<TData>(TData value, [CallerMemberName] string? name = null)
            => _propertys.AddOrUpdate(new InternalData(value, name!));

        protected TData GetValue<TData>(TData defaultData, [CallerMemberName] string? name = null)
            #pragma warning disable EPS06
            => _propertys.Lookup(name!).ConvertOr(id => id?.Data is TData data ? data : defaultData, () => defaultData)!;
        #pragma warning restore EPS06

        public string InternalError
        {
            get => GetValue(string.Empty);
            set => SetValue(value);
        }

        protected IObserver<TData> PropertyObserver<TData>(Expression<Func<TData>> property)
        {
            string name = Reflex.PropertyName(property);

            return Observer.Create<TData>(v => SetValue(v, name), ErrorEncountered);
        }

        protected void AddValidation<TData>(Expression<Func<TData>> property, Func<IObservable<TData?>, IObservable<string>> validate)
        {
            var name = Reflex.PropertyName(property);

            _propertys.Connect()
                      .Where(o => o.Name == name)
                      .Flatten()
                      .SelectMany(c =>
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
                                                         o.When(string.IsNullOrWhiteSpace,
                                                             s => s.Select(_ => new Action(() => _errors.RemoveKey(name))));
                                                         o.When(e => !string.IsNullOrWhiteSpace(e),
                                                             no => no.NotEmpty()
                                                                     .Select(e => new Action(() => _errors.AddOrUpdate(new ValidationError(e, name)))));
                                                     });
                                      }

                                      return c.Reason switch
                                      {
                                          ChangeReason.Add => CreateChangeAction(),
                                          ChangeReason.Update => CreateChangeAction(),
                                          ChangeReason.Remove => CreateEmpty(),
                                          _ => CreateEmpty()
                                      };
                                  })
                      .AutoSubscribe(a => a(), ErrorEncountered)
                      .DisposeWith(Disposer);
        }

        public virtual void ErrorEncountered(Exception exception)
        {
            InternalError = exception.Message;
            _errors.AddOrUpdate(new ValidationError(exception.Message, "InternalError"));
        }

        IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                foreach (var error in _errors.Items)
                {
                    yield return error.Error;
                }

                yield break;
            }

            var opt = _errors.Lookup(propertyName);
            if(!opt.HasValue) yield break;

            yield return opt.Value.Error;
        }

        bool INotifyDataErrorInfo.HasErrors => _errors.Count != 0;

        private event EventHandler<DataErrorsChangedEventArgs>? ErrorsChangedInternal;

        event EventHandler<DataErrorsChangedEventArgs>? INotifyDataErrorInfo.ErrorsChanged
        {
            add => ErrorsChangedInternal += value;
            remove => ErrorsChangedInternal -= value;
        }

        public IObservable<string> ErrorsChanged { get; }

        IObservable<string> IObservablePropertyChanged.PropertyChangedObservable 
            => _propertys.Connect().Flatten().Select(d => d.Key);

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void Dispose()
        {
            _errors.Dispose();
            _propertys.Dispose();
            Disposer.Dispose();
        }
        
        private sealed record ValidationError(string Error, string Property);

        private sealed record InternalData(object? Data, string Name);
    }
}