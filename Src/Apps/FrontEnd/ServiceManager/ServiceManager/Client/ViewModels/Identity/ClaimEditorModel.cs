using System.Collections.Generic;
using System.Collections.Immutable;
using ServiceManager.Shared.Identity;

namespace ServiceManager.Client.ViewModels.Identity
{
    public sealed class ClaimEditorModel
    {
        private static readonly Dictionary<string, string> ClaimsDisplayNames
            = new()
              {
                  {Claims.ConfigurationClaim, "App Konfiguration"},
                  {Claims.DatabaseClaim, "Datenbank Konfiguration"},
                  {Claims.AppIpClaim, "Ip Konfiguration"},
                  {Claims.ClusterConnectionClaim, "Cluster Verbindung"},
                  {Claims.ClusterNodeClaim, "Cluster Status"},
                  {Claims.ServerInfoClaim, "Server Status"},
                  {Claims.UserManagmaentClaim, "Benutzer Verwaltung"}
              };

        public static string GetClaimsDisplayName(string name)
            => ClaimsDisplayNames.GetValueOrDefault(name, "Unbekannter Claim");

        public string OriginalName { get; set; }
        
        public string Name { get; }

        public bool IsChecked { get; set; }

        public ClaimEditorModel(string name, bool isChecked)
        {
            OriginalName = name;
            Name         = GetClaimsDisplayName(name);
            IsChecked    = isChecked;
        }

        public ImmutableArray<string> SetClaim(ImmutableArray<string> array)
            => IsChecked ? array.Add(OriginalName) : array;
    }
}