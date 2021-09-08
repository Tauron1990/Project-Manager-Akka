using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MudBlazor;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Client.Shared.Configuration.ConditionEditor;
using ServiceManager.Client.Shared.Dialog;
using ServiceManager.Shared.ServiceDeamon;
using Stl.Fusion;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{

    public sealed record AppConfigData(AppConfigModel? NewModel, AppConfigModel? ToEdit)
    {
        [UsedImplicitly]
        public AppConfigData()
            : this(null, null) { }
    }
    
    public sealed class ConfigurationViewAppConfigModel
    {
        private readonly IMutableState<AppConfigData> _viewState;
        private readonly IServerConfigurationApi      _api;
        private readonly IDialogService               _dialogService;
        private readonly IEventAggregator             _aggregator;
        private readonly EditorState _editorState;

        public ConfigurationViewAppConfigModel(IMutableState<AppConfigData> viewState, IServerConfigurationApi api, IDialogService dialogService, IEventAggregator aggregator,
                                               EditorState editorState)
        {
            _viewState = viewState;
            _api            = api;
            _dialogService  = dialogService;
            _aggregator     = aggregator;
            _editorState = editorState;
        }

        public async Task CommitConfig(AppConfigModel model)
        {
            bool reset = false;
            try
            {
                if (_editorState.ChangesWhereMade)
                {
                    var saveCon = await _dialogService.ShowMessageBox("Bedingungen geändert", "Die Bedingungen wurden geändert. Speichern?", "Ja", "Nein", "Abbrechen");
                    switch (saveCon)
                    {
                        case null:
                            _aggregator.PublishWarnig("Vorgang Abbgebrochen");
                            return;
                        case true:
                            if (!_editorState.CommitChanges(model, _aggregator))
                                return;
                            break;
                    }
                }

                var validResult = model.Validatebasic();
                if (!string.IsNullOrWhiteSpace(validResult))
                {
                    _aggregator.PublishWarnig(validResult);
                    return;
                }
                
                var data = model.CreateNew();

                model.IsNew = false;
                reset = await _aggregator.IsSuccess(() => _api.UpdateSpecificConfig(new UpdateSpecifcConfigCommand(data)));
                if(reset)
                    _aggregator.PublishSuccess($"Änderungen angewended für {data.Name}");
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
            finally
            {
                if(reset)
                    _viewState.Set(_viewState.Value with{ ToEdit = null, NewModel = null });
            }
        }
        
        public async Task NewConfig(ImmutableList<AppConfigModel> appConfigs)
        {
            try
            {
                var dialog = _dialogService.Show<NewAppConfigDialog>(
                    String.Empty,
                    new DialogParameters
                    {
                        {
                            "Blocked", appConfigs.Select(m => m.Config.Id)
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
                     && appConfigs.All(m => !m.Config.Id.Equals(name, StringComparison.Ordinal)))
                    {
                        var model = new AppConfigModel(new SpecificConfig(name, string.Empty, string.Empty, ImmutableList<Condition>.Empty), _aggregator)
                                    {
                                        IsNew = true
                                    };

                        _viewState.Set(_viewState.Value with{ NewModel = model });
                        EditConfig(model);
                    }
                    else
                        _aggregator.PublishWarnig("Der name nicht Valid");
                }
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
                Console.WriteLine(e);
            }
        }

        public async Task DeleteConfig(AppConfigModel? model)
        {
            if (model == null) return;

            try
            {
                if (await _aggregator.IsSuccess(() => _api.DeleteSpecificConfig(new DeleteSpecificConfigCommand(model.Config.Id))))
                {
                    _aggregator.PublishSuccess($"Konfiguration {model.Config.Id} erfolgreich gelöscht");
                    _viewState.Set(_viewState.Value with { ToEdit = null, NewModel = null });
                }
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        public void EditConfig(AppConfigModel? model)
        {
            _editorState.Reset();
            _viewState.Set(_viewState.Value with { ToEdit = model, NewModel = null });
        }
    }
}