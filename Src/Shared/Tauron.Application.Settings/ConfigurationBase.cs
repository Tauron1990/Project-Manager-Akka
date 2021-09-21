using System;
using System.Collections.Immutable;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Tauron.Akka;

namespace Tauron.Application.Settings
{
    [PublicAPI]
    public abstract class ConfigurationBase : ObservableObject
    {
        private readonly IDefaultActorRef<SettingsManager> _actor;
        private readonly Task _loader;
        private readonly ILogger _logger;
        private readonly string _scope;

        private ImmutableDictionary<string, string> _dic = ImmutableDictionary<string, string>.Empty;

        private bool _isBlocked;

        protected ConfigurationBase(IDefaultActorRef<SettingsManager> actor, string scope, ILogger logger)
        {
            _actor = actor;
            _logger = logger;

            if (actor is EmptyActor<SettingsManager>)
            {
                _scope = string.Empty;
                _loader = Task.CompletedTask;
            }
            else
            {
                _scope = scope;
                _loader = Task.Run(async () => await LoadValues());
            }
        }

        public IDisposable BlockSet()
        {
            _isBlocked = true;

            return Disposable.Create(this, self => self._isBlocked = false);
        }

        private async Task LoadValues()
            => _dic = await _actor.Ask<ImmutableDictionary<string, string>>(new RequestAllValues(_scope));

        protected TValue? GetValue<TValue>(Func<string, TValue> converter, TValue? defaultValue = default, [CallerMemberName] string? name = null)
        {
            try
            {
                _loader.Wait();

                if (string.IsNullOrEmpty(name)) return default!;

                return _dic.TryGetValue(name, out var value) ? converter(value) : defaultValue;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on  Load Configuration Data");

                return defaultValue;
            }
        }

        protected void SetValue(string value, [CallerMemberName] string? name = null)
        {
            if (_isBlocked)
                return;

            if (string.IsNullOrEmpty(name)) return;

            _dic = _dic.Add(value, name);

            _actor.Tell(new SetSettingValue(_scope, name, value));

            OnPropertyChanged(name);
        }
    }
}