using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Authorization;
using ServiceManager.Shared.Identity;

namespace ServiceManager.Client.ViewModels.Identity
{
    public sealed class ErrorMessageProvider : IErrorMessageProvider
    {
        public IEnumerable<string> GetMessage(AuthenticationState state, string[] roles)
            => from role in roles
               where !state.User.IsInRole(role)
               select role switch
               {
                   Claims.AppIpClaim => "Keine Berechtigung für den zugriff aud Ip der Anwednung",
                   Claims.ClusterConnectionClaim => "Keine Berechtigung für den zurgiff auf den Cluster",
                   Claims.ClusterNodeClaim => "Keine Berechtigung für den zurgiff auf den Cluster",
                   Claims.ConfigurationClaim => "Keine Berechtigung für den zugriff auf die Cluster Konfiguration",
                   Claims.DatabaseClaim => "Keine Berechtigung für den zugriff auf die Cluster Datenbank",
                   Claims.ServerInfoClaim => "Keine Berechtingung zu Abrufen von Server Infos",
                   Claims.UserManagmaentClaim => "Keine Berechtigung zum User Management",
                   Claims.AppMenegmentClaim => "Keine Berechtigung zur Anwendungs Verwaltung",
                   _ => $"Unbekante Verletzung \"{role}\""
               };
    }
}