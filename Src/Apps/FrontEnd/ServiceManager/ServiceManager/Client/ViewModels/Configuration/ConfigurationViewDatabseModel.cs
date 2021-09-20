using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hyperion.Internal;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using ServiceManager.Client.Shared.Dialog;
using ServiceManager.Shared;
using ServiceManager.Shared.ServiceDeamon;
using Tauron.Application;

namespace ServiceManager.Client.ViewModels.Configuration
{
    public sealed record ConfigurationViewDatabseModel(IServerInfo ServerInfo,  IDialogService DialogService, IEventAggregator Aggregator, 
                                                       string         OriginalUrl,   IDatabaseConfig  DatabaseConfig, string DatabaseUrl,
                                                       bool        CanFetch,    Func<string, string?> ValidateUrl, bool IsValid)
    {
        [ActivatorUtilitiesConstructor, UsedImplicitly]
        public ConfigurationViewDatabseModel(IServerInfo serverInfo, IDialogService dialogService, IEventAggregator aggregator, IDatabaseConfig config)
            : this(serverInfo, dialogService, aggregator, string.Empty, config, string.Empty, CanFetch: false, Validate, IsValid: false)
        {
            
        }

        public ConfigurationViewDatabseModel Reset() 
            => this with{ DatabaseUrl = OriginalUrl };

        public async Task Submit()
        {
            if(!IsValid) return; 
            try
            {
                var diag = await DialogService.Show<ConfirmRestartDialog>().Result;

                if (diag.Cancelled)
                {
                    Aggregator.PublishWarnig("Vorgang Abgebrochen");

                    return;
                }

                var result = await DatabaseConfig.SetUrl(new SetUrlCommand(DatabaseUrl));
                if (string.IsNullOrWhiteSpace(result))
                {
                    await Task.Delay(1000);
                    await ServerInfo.Restart(new RestartCommand());

                    return;
                }

                Aggregator.PublishWarnig($"Fehler: {result}");
            }
            catch (Exception e)
            {
                Aggregator.PublishError(e);
            }
        }
        
        public async Task<(ConfigurationViewDatabseModel Model, bool Success)> TryFetchDatabseUrl()
        {
            try
            {
                var urlResult = await DatabaseConfig.FetchUrl();

                if (urlResult == null)
                    Aggregator.PublishError("Unbekannter Fehler beim Abrufen der Url");
                else if (urlResult.Success)
                    return (this with{ DatabaseUrl = urlResult.Url }, true);
                else
                    Aggregator.PublishError($"Fehler beim Abrufen der Url: {urlResult.Url}");
            }
            catch (Exception e)
            {
                Aggregator.PublishError(e);
            }

            return (this, false);
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
    }
}