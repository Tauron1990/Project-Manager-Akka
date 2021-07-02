using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceManager.Client.Components;
using ServiceManager.Shared.Events;

namespace ServiceManager.Client.Model
{
    public class ModelBase : ObservableObject
    {
        private readonly HttpClient _client;
        private readonly NavigationManager _manager;
        private readonly HubConnection _hubConnection;
        private bool _isInit;

        public ModelBase(HttpClient client, NavigationManager manager, HubConnection hubConnection)
        {
            _client = client;
            _manager = manager;
            _hubConnection = hubConnection;
        }

        private async Task OnPropertyChanged<TType>(string type, string property, Action<TType> setter, Func<HttpClient, Task<TType>> query)
        {
            setter(await query(_client));

            _hubConnection.On<string, string>(HubEvents.PropertyChanged,
                async (msgType, msgName) =>
                {
                    if (msgType.Equals(type, StringComparison.Ordinal) && msgName.Equals(property, StringComparison.Ordinal))
                    {
                        setter(await query(_client));
                        OnPropertyChanged(property);
                    }
                });
        }

        private void OnMessage(string type, string name, Func<Task> runner)
        {
            _hubConnection.On<string, string>(HubEvents.PropertyChanged,
                async (msgType, msgName) =>
                {
                    if (msgType.Equals(type, StringComparison.Ordinal) && msgName.Equals(name, StringComparison.Ordinal))
                        await runner();
                });
        }
        
        protected async Task Init(Action<Configuration> config)
        {
            if(_isInit) return;
            _isInit = true;

            Func<Task> updateRunner = () => Task.CompletedTask;

            config(new Configuration(this, func => updateRunner = func));
            await _hubConnection.StartAsync();
            await updateRunner();
        }

        protected sealed class Configuration
        {
            private readonly ModelBase _self;
            private readonly List<Func<Task>> _initList = new();

            public Configuration(ModelBase self, Action<Func<Task>> updater)
            {
                _self = self;
                updater(Init);
            }

            private async Task Init()
            {
                foreach (var func in _initList) await func();
            }

            public void ForInterface<TInterface>(Action<InterfaceConfiguration<TInterface>> config) 
                => config(new InterfaceConfiguration<TInterface>(_self, func => _initList.Add(func)));

            public sealed class InterfaceConfiguration<TInterface>
            {
                private readonly ModelBase _self;
                private readonly List<Func<Task>> _updater = new();

                public InterfaceConfiguration(ModelBase self, Action<Func<Task>> initFactory)
                {
                    _self = self;
                    initFactory(Init);
                }

                private async Task Init()
                {
                    foreach (var func in _updater) 
                        await func();
                }

                public void OnPropertyChanged<TType>(Expression<Func<TInterface, TType>> property, Action<TType> setter, Func<HttpClient, Task<TType>> query)
                    => _updater.Add(() => _self.OnPropertyChanged(
                                        typeof(TInterface).AssemblyQualifiedName!,
                                        ((MemberExpression) property.Body).Member.Name,
                                        setter, query));

                public void OnMessage(string name, Func<Task> runner) 
                    => _self.OnMessage(typeof(TInterface).AssemblyQualifiedName!, name, runner);
            }
        }
    }
}