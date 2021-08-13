using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MudBlazor;
using ServiceManager.Client.Components;
using ServiceManager.Client.Components.Operations;
using ServiceManager.Client.Shared.Dialog;
using ServiceManager.Client.ViewModels.Models;
using ServiceManager.Shared;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewDatabseModel : ObservableObject, IDisposable, IInitable
    {
        private readonly IDatabaseConfigOld _databaseConfig;
        private readonly IServerInfo _info;
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _aggregator;
        private readonly IDisposable _disposable;

        private string _databaseUrl = string.Empty;
        private string _originalUrl = string.Empty;

        public string DatabaseUrl
        {
            get => _databaseUrl;
            set => SetProperty(ref _databaseUrl, value);
        }

        public string OriginalUrl
        {
            get => _originalUrl;
            set => SetProperty(ref _originalUrl, value);
        }

        public Func<string, string?> ValidateUrl { get; } = Validate;

        public IOperationManager Operation { get; set; } = OperationManager.Empty;

        public ConfigurationViewDatabseModel(IDatabaseConfigOld databaseConfig, IServerInfo info, IDialogService dialogService, IEventAggregator aggregator)
        {
            _databaseConfig = databaseConfig;
            _info = info;
            _dialogService = dialogService;
            _aggregator = aggregator;
            _disposable = databaseConfig.PropertyChangedObservable
                                        .Subscribe(s =>
                                                   {
                                                       if (s == nameof(databaseConfig.Url))
                                                           SetUrl(databaseConfig.Url);
                                                   });

            SetUrl(databaseConfig.Url);
        }

        public void Reset() => DatabaseUrl = OriginalUrl;

        public async Task Submit()
        {
            using (Operation.Start())
            {
                try
                {
                    var diag = await _dialogService.Show<ConfirmRestartDialog>().Result;

                    if (diag.Cancelled)
                    {
                        _aggregator.PublishWarnig("Vorgang Abgebrochen");
                        return;
                    }

                    var result = await _databaseConfig.SetUrl(DatabaseUrl);
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        await Task.Delay(1000);
                        await _info.Restart(new RestartCommand());
                        return;
                    }

                    _aggregator.PublishWarnig($"Fehler: {result}");
                }
                catch (Exception e)
                {
                    _aggregator.PublishError(e);
                }
            }
        }

        public async Task Init()
        {
            if (_databaseConfig is ModelBase mb)
                await mb.Init();
        }

        public async Task TryFetchDatabseUrl()
        {
            try
            {
                var urlResult = await _databaseConfig.FetchUrl();

                if (urlResult == null)
                    _aggregator.PublishError("Unbekannter Fehler beim Abrufen der Url");
                else if (urlResult.Success)
                    DatabaseUrl = urlResult.Url;
                else
                    _aggregator.PublishError($"Fehler beim Abrufen der Url: {urlResult.Url}");
            }
            catch (Exception e)
            {
                _aggregator.PublishError(e);
            }
        }

        private void SetUrl(string url)
        {
            DatabaseUrl = url;
            if (string.IsNullOrWhiteSpace(_originalUrl))
                OriginalUrl = url;
        }

        private static string? Validate(string originalConnectionString)
        {
            const string serverPattern = @"(?<host>((\[[^]]+?\]|[^:@,/?#]+)(:\d+)?))";
            const string serversPattern = serverPattern + @"(," + serverPattern + ")*";
            const string databasePattern = @"(?<database>[^/?]+)";
            const string optionPattern = @"(?<option>[^&;]+=[^&;]+)";
            const string optionsPattern = @"(\?" + optionPattern + @"((&|;)" + optionPattern + ")*)?";
            const string pattern =
                @"^(?<scheme>mongodb|mongodb\+srv)://" +
                @"((?<username>[^:@/]+)(:(?<password>[^:@/]*))?@)?" +
                serversPattern + @"(/" + databasePattern + ")?/?" + optionsPattern + "$";

            if (string.IsNullOrWhiteSpace(originalConnectionString))
                return "Keine Mogodb Url Angegeben";

            if (originalConnectionString.Contains("%"))
            {
                const string invalidPercentPattern = @"%$|%.$|%[^0-9a-fA-F]|%[0-9a-fA-F][^0-9a-fA-F]";
                if (Regex.IsMatch(originalConnectionString, invalidPercentPattern))
                {
                    var protectedConnectionString = ProtectConnectionString(originalConnectionString);
                    return $"Der Connection string '{protectedConnectionString}' enthält eine invalide '%' Escape Sequenz.";
                }
            }

            var match = Regex.Match(originalConnectionString, pattern);
            if (!match.Success)
            {
                var protectedConnectionString = ProtectConnectionString(originalConnectionString);
                return $"Der Connection string '{protectedConnectionString}' ist nicht gültig.";
            }

            string ProtectConnectionString(string connectionString)
            {
                var protectedString = Regex.Replace(connectionString, @"(?<=://)[^/]*(?=@)", "<hidden>");
                return protectedString;
            }

            return null;
        }

        public void Dispose() => _disposable.Dispose();
    }
}