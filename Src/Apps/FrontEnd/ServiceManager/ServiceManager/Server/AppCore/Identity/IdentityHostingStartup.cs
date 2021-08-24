using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceManager.Server.AppCore.Identity;
using ServiceManager.Server.AppCore.Identity.Data;
using ServiceManager.Shared.Identity;

[assembly: HostingStartup(typeof(IdentityHostingStartup))]
namespace ServiceManager.Server.AppCore.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(
                (context, services) =>
                {
                    services.AddDbContext<ServiceManagerServerContext>(
                        options =>
                            options.UseSqlite(
                                context.Configuration.GetConnectionString("ServiceManagerServerContextConnection")));

                    services.AddAuthentication().AddJwtBearer();

                    services.AddDefaultIdentity<ServiceManagerServerUser>(options => options.SignIn.RequireConfirmedAccount = true)
                            .AddEntityFrameworkStores<ServiceManagerServerContext>();

                    services.AddAuthorization(
                        o =>
                        {
                            o.AddPolicy(Claims.AppIpClaim, b => b.RequireClaim(ClaimTypes.Role, Claims.AppIpClaim));
                            o.AddPolicy(Claims.ClusterConnectionClaim, b => b.RequireClaim(ClaimTypes.Role, Claims.ClusterConnectionClaim));
                            o.AddPolicy(Claims.ClusterNodeClaim, b => b.RequireClaim(ClaimTypes.Role, Claims.ClusterNodeClaim));
                            o.AddPolicy(Claims.ConfigurationClaim, b => b.RequireClaim(ClaimTypes.Role, Claims.ConfigurationClaim));
                            o.AddPolicy(Claims.DatabaseClaim, b => b.RequireClaim(ClaimTypes.Role, Claims.DatabaseClaim));
                            o.AddPolicy(Claims.ServerInfoClaim, b => b.RequireClaim(ClaimTypes.Role, Claims.ServerInfoClaim));
                        });
                });
        }
    }
}