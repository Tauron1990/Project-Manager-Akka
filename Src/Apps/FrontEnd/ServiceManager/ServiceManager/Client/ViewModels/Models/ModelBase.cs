﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceManager.Client.Components;
using ServiceManager.Client.ViewModels.Events;
using ServiceManager.Shared.Api;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Models
{
    public abstract class ModelBase : ObservableObject, IInitable, IDisposable
    {
        private readonly IDisposable _subscription;

        protected HttpClient Client { get; }
        protected HubConnection HubConnection { get; }
        protected IEventAggregator Aggregator { get; }

        private bool _isInit;

        protected ModelBase(HttpClient client, HubConnection hubConnection, IEventAggregator aggregator)
        {
            Client = client;
            HubConnection = hubConnection;
            Aggregator = aggregator;
            _subscription = aggregator.GetEvent<ReloadAllEvent, Unit>().Get().Subscribe(_ => _isInit = false);
        }

        private async Task OnPropertyChanged<TType>(string type, string property, Action<TType> setter, Func<HttpClient, Task<TType>> query)
        {
            try
            {
                HubConnection.On<string, string>(HubEvents.PropertyChanged,
                    async (msgType, msgName) =>
                    {
                        try
                        {
                            Console.WriteLine($"OnPropertyChanged Update: {type} -- {property}");
                            if (msgType.Equals(type, StringComparison.Ordinal) && msgName.Equals(property, StringComparison.Ordinal))
                            {
                                setter(await query(Client));
                                OnPropertyChanged(property);
                            }
                        }
                        catch (Exception e)
                        {
                            Aggregator.PublishError(e);
                            _isInit = false;
                        }
                    });

                Console.WriteLine($"OnPropertyChanged Init: {type} -- {property}");
                setter(await query(Client));
                Console.WriteLine($"OnPropertyChanged Init Compled: {type} -- {property}");
            }
            catch (Exception e)
            {
                Aggregator.PublishError(e);
                _isInit = false;
            }
        }

        private void OnMessage(string type, string name, Func<Task> runner)
        {
            HubConnection.On<string, string>(HubEvents.PropertyChanged,
                async (msgType, msgName) =>
                {
                    if (msgType.Equals(type, StringComparison.Ordinal) && msgName.Equals(name, StringComparison.Ordinal))
                        await runner();
                });
        }

        public abstract Task Init();

        protected async Task Init(Action<Configuration> config)
        {
            if(_isInit) return;
            _isInit = true;

            Func<Task> updateRunner = () => Task.CompletedTask;

            config(new Configuration(this, func => updateRunner = func));
            if(HubConnection.State == HubConnectionState.Disconnected)
                await HubConnection.StartAsync();
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

                //public void OnPropertyChanged<TType>(Expression<Func<TInterface, TType>> property, Action<TType> setter, Func<HttpClient, Task<TType>> query)
                //    => _updater.Add(() => _self.OnPropertyChanged(
                //                        typeof(TInterface).FullName!,
                //                        ((MemberExpression) property.Body).Member.Name,
                //                        setter, query));

                public void OnPropertyChanged<TType, TResponse>(Expression<Func<TInterface, TType>> property, Action<TResponse> setter, Func<HttpClient, Task<TResponse>> query)
                    => _updater.Add(() => _self.OnPropertyChanged(
                                        typeof(TInterface).FullName!,
                                        ((MemberExpression)property.Body).Member.Name,
                                        setter, query));
                
                public void OnPropertyChanged<TResponse>(string property, Action<TResponse> setter, Func<HttpClient, Task<TResponse>> query)
                    => _updater.Add(() => _self.OnPropertyChanged(
                                        typeof(TInterface).FullName!,
                                        property,
                                        setter, query));

                public void OnMessage(string name, Func<Task> runner) 
                    => _self.OnMessage(typeof(TInterface).AssemblyQualifiedName!, name, runner);
            }
        }

        public virtual void Dispose() => _subscription.Dispose();
    }
}