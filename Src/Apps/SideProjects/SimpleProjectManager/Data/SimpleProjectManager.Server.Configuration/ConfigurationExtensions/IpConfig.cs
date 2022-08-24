using System.Collections.Immutable;
using Microsoft.AspNetCore.Hosting;

namespace SimpleProjectManager.Server.Configuration.ConfigurationExtensions;

public sealed class IpConfig : ConfigExtension
{
    private const string ServerIp = "serverIp";
    private const string Host = "host";

    private const string HostLocal = "local";
    private const string HostIp = "ip";
    private const string HostBoth = "both";
    
    public override void Apply(ImmutableDictionary<string, string> propertys, IWebHostBuilder applicationBuilder)
    {
        if(!propertys.ContainsKey(ServerIp))
            return;

        if(propertys.ContainsKey(Host))
        {
            switch (propertys[Host])
            {
                case HostLocal: 
                case HostIp:
                    applicationBuilder.UseUrls(propertys[ServerIp]);
                    break;
                case HostBoth:
                    applicationBuilder.UseUrls(propertys[ServerIp], $"http://localhost:{propertys.GetValueOrDefault("severPort", "4000")}");
                    break;
            }
        }
        else
            applicationBuilder.UseUrls(propertys[ServerIp]);
    }
}