using Microsoft.Extensions.Configuration;

namespace ServiceHost.Installer
{
    public sealed record InstallChecker
    {
        public bool IsInstallationStart { get; }

        public InstallChecker(IConfiguration configuration)
        {
            try
            {
                IsInstallationStart = configuration["Install"]?.ToLower() == "manual";
            }
            catch
            {
                IsInstallationStart = false;
                #pragma warning disable ERP022
            }
            #pragma warning restore ERP022
        }
    }
}