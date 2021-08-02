using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MudBlazor;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Components;
using ServiceManager.Client.Shared.Dialog;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ServiceDeamon;
using Tauron;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewAppConfigModel : ObservableObject, IInitable, IDisposable
    {
        private readonly IServerConfigurationApi _api;
        private readonly IDialogService          _dialogService;
        private readonly IEventAggregator        _aggregator;
        private readonly IDisposable             _subscription;

        private AppConfigModel?               _toEdit;
        private bool                          _isLoading = true;
        private ImmutableList<AppConfigModel> _appConfigs;

        public AppConfigModel? ToEdit
        {
            get => _toEdit;
            private set => SetProperty(ref _toEdit, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public ImmutableList<AppConfigModel> AppConfigs
        {
            get => _appConfigs;
            private set
            {
                SetProperty(ref _appConfigs, value);
                ToEdit = null;
            }
        }

        public ConfigurationViewAppConfigModel(IServerConfigurationApi api, IDialogService dialogService, IEventAggregator aggregator)
        {
            _api           = api;
            _dialogService = dialogService;
            _aggregator    = aggregator;
            _appConfigs    = ImmutableList<AppConfigModel>.Empty;

            _subscription = (from prop in api.PropertyChangedObservable
                             where prop == HubEvents.AppsConfigChanged
                             from config in api.QueryAppConfig()
                             select (from specificConfig in config
                                     select new AppConfigModel(specificConfig, _aggregator))
                                .ToImmutableList())
               .AutoSubscribe(
                    config =>
                    {
                        ToEdit     = null;
                        AppConfigs = config;
                    });
        }

        public async Task Init()
        {
            try
            {
                await PropertyChangedComponent.Init(_api);
                AppConfigs = (await _api.QueryAppConfig())
                   .Select(c => new AppConfigModel(c, _aggregator))
                   .ToImmutableList();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task CommitConfig(AppConfigModel model)
        {
            try
            {
                var data = model.CreateNew();

                model.IsNew = false;
                var result = await _api.Update(data);
                if(string.IsNullOrWhiteSpace(result)) return;
                
                _aggregator.PublishWarnig(result);
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
            finally
            {
                ToEdit = null;
            }
        }
        
        public async Task NewConfig()
        {
            try
            {
                var dialog = _dialogService.Show<NewAppConfigDialog>(
                    String.Empty,
                    new DialogParameters
                    {
                        {
                            "Blocked", AppConfigs.Select(m => m.Config.Id)
                               .ToImmutableHashSet()
                        }
                    });

                var result = await dialog.Result;
                if (result.Cancelled)
                    _aggregator.PublishWarnig("Der Vorgang wurde Abgebrochen");
                else
                {
                    if (result.Data is string name
                     && !string.IsNullOrWhiteSpace(name)
                     && _appConfigs.All(m => !m.Config.Id.Equals(name, StringComparison.Ordinal)))
                    {
                        var model = new AppConfigModel(new SpecificConfig(name, string.Empty, string.Empty, ImmutableList<Condition>.Empty), _aggregator)
                                    {
                                        IsNew = true
                                    };

                        AppConfigs = AppConfigs.Add(model);
                        EditConfig(model);
                    }
                    else
                        _aggregator.PublishWarnig("Der name nicht Valid");
                }
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public async Task DeleteConfig(AppConfigModel? model)
        {
            if (model == null) return;

            try
            {
                var result = await _api.DeleteSpecificConfig(model.Config.Id);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    _aggregator.PublishWarnig(result);

                    return;
                }

                ToEdit     = null;
                AppConfigs = AppConfigs.Remove(model);
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public void EditConfig(AppConfigModel? model)
            => ToEdit = model;

        public void Dispose()
            => _subscription.Dispose();
    }
}