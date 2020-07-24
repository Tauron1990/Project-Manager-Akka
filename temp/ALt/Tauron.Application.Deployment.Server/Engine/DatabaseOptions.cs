﻿using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Tauron.Application.OptionsStore;

namespace Tauron.Application.Deployment.Server.Engine
{
    public sealed class DatabaseOptions : INotifyPropertyChanged
    {
        private const string IsSetupFinisht = nameof(IsSetupFinisht);
        private readonly ConcurrentDictionary<string, IOption> _options = new ConcurrentDictionary<string, IOption>();
        private readonly IAppOptions _store;

        public DatabaseOptions(IOptionsStore store)
            => _store = store.GetAppOptions("DeploymentServer");
        
        public event PropertyChangedEventHandler PropertyChanged;

        private async Task<string> GetValueAsync(string name)
        {
            if (_options.TryGetValue(name, out var opt))
                return opt.Value;

            opt = await _store.GetOptionAsync(name);

            _options[name] = opt;
            return opt.Value;
        }

        private async Task SetValueAsync(string name, string value)
        {
            if (_options.TryGetValue(name, out var opt))
            {
                await opt.SetValueAsync(value);
                return;
            }

            opt = await _store.GetOptionAsync(name);

            _options[name] = opt;
            await opt.SetValueAsync(value);
        }


        private string GetValue(string name)
        {
            if (_options.TryGetValue(name, out var opt))
                return opt.Value;

            opt = _store.GetOption(name);

            _options[name] = opt;
            return opt.Value;
        }

        private void SetValue(string name, string value)
        {
            if (_options.TryGetValue(name, out var opt))
            {
                opt.SetValue(value);
                return;
            }

            opt = _store.GetOption(name);

            _options[name] = opt;
            opt.SetValue(value);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}