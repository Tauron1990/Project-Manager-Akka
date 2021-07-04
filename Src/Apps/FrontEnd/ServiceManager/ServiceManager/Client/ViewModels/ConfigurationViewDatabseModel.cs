using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MudBlazor;
using ServiceManager.Client.Components;
using ServiceManager.Client.Shared.Dialog;
using ServiceManager.Client.ViewModels.Models;
using ServiceManager.Shared;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels
{
    public sealed class ConfigurationViewDatabseModel : ObservableObject, IDisposable, IInitable
    {
        private readonly IDatabaseConfig _databaseConfig;
        private readonly ISnackbar _snackbar;
        private readonly IServerInfo _info;
        private readonly IDialogService _dialogService;
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

        public ConfigurationViewDatabseModel(IDatabaseConfig databaseConfig, ISnackbar snackbar, IServerInfo info, IDialogService dialogService)
        {
            _databaseConfig = databaseConfig;
            _snackbar = snackbar;
            _info = info;
            _dialogService = dialogService;
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
            var diag = await _dialogService.Show<ConfirmRestartDialog>().Result;

            if (diag.Cancelled)
            {
                _snackbar.Add("Vorgang Abgebrochen", Severity.Warning);
                return;
            }

            var result = await _databaseConfig.SetUrl(DatabaseUrl);
            if (string.IsNullOrWhiteSpace(result))
            {
                await _info.Restart();
                return;
            }

            _snackbar.Add($"Fehler: {result}", Severity.Warning);
        }

        public async Task Init()
        {
            if (_info is IInitable initable)
                await initable.Init();
            if (_databaseConfig is ModelBase mb)
                await mb.Init();
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