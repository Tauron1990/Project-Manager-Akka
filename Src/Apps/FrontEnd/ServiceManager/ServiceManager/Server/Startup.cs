using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Akka.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Server.AppCore.ClusterTracking.Data;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Server.AppCore.Identity;
using ServiceManager.Server.AppCore.ServiceDeamon;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.Identity;
using ServiceManager.Shared.ServiceDeamon;
using Stl.CommandR;
using Stl.Fusion;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.Server;
using Tauron;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.MongoExtensions;

namespace ServiceManager.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        private IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            ImmutableListSerializer<Condition>.Register();
            ImmutableListSerializer<SeedUrl>.Register();


            services.AddDbContextFactory<UsersDatabase>(
                builder =>
                {
                    var targetPath = Path.Combine(Program.ExeFolder, "_database");
                    targetPath.CreateDirectoryIfNotExis();

                    var source = new SqliteConnectionStringBuilder
                                 {
                                     DataSource = Path.Combine(targetPath, "data.db")
                                 }.ConnectionString;

                    builder.UseSqlite(source);
                });

            services.AddCommander()
                    .AddCommandService<INodeUpdateHandler, NodeUpdateHandler>();

            services.AddDbContextServices<UsersDatabase>(
                o => o
                    .AddFileBasedOperationLogChangeTracking(Path.Combine(Program.ExeFolder, "_changed"))
                    .AddOperations()
                    .AddAuthentication<SessionInfoEntity, UserEntity, long>());

            var fusion = services.AddFusion();

            services.AddAuthentication(o => o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(
                         options =>
                         {
                             options.LoginPath = "/signIn";
                             options.LogoutPath = "/signOut";
                             options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                         });
            fusion.AddAuthentication(
                o => o.AddServer());

            fusion.AddComputeService<IClusterNodeTracking, ClusterNodeTracking>()
                  .AddComputeService<IClusterConnectionTracker, ClusterConnectionTracker>()
                  .AddComputeService<IServerInfo, ServerInfo>()
                  .AddComputeService<IAppIpManager, AppIpService>()
                  .AddComputeService<IDatabaseConfig, DatabaseConfig>()
                  .AddComputeService<IServerConfigurationApi, ServerConfigurationApi>();


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
            
            services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(30));

            services.Configure<IdentityOptions>(options =>
                                                {
                                                    // Password settings.
                                                    options.Password.RequireDigit = false;
                                                    options.Password.RequireLowercase = false;
                                                    options.Password.RequireNonAlphanumeric = false;
                                                    options.Password.RequireUppercase = false;
                                                    options.Password.RequiredLength = 3;
                                                    options.Password.RequiredUniqueChars = 0;

                                                    // Lockout settings.
                                                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                                                    options.Lockout.MaxFailedAccessAttempts = 5;
                                                    options.Lockout.AllowedForNewUsers = true;

                                                    // User settings.
                                                    options.User.AllowedUserNameCharacters =
                                                        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                                                    options.User.RequireUniqueEmail = false;
                                                });

            services.ConfigureApplicationCookie(options =>
                                                {
                                                    // Cookie settings
                                                    options.Cookie.HttpOnly = true;
                                                    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                                                    options.SlidingExpiration = true;
                                                });
            services.AddCors();
            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCompression(
                opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));
        }


        [UsedImplicitly]
        public void ConfigureContainer(IActorApplicationBuilder builder)
        {
            builder.OnMemberRemoved(
                        (_, system, _) =>
                        {
                            try
                            {
                                var resolverScope = DependencyResolver.For(system).Resolver.CreateScope();
                                var resolver = resolverScope.Resolver;

                                resolver.GetService<IHubContext<ClusterInfoHub>>().Clients.All
                                        .SendAsync(HubEvents.RestartServer)
                                        .ContinueWith(
                                             _ =>
                                             {
                                                 using (resolverScope)
                                                 {
                                                     resolver.GetService<IRestartHelper>().Restart = true;
                                                     resolver.GetService<IHostApplicationLifetime>().StopApplication();
                                                 }
                                             });
                            }
                            catch (ObjectDisposedException) { }
                        })
                   .AddModule<MainModule>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseCors(builder => builder.WithFusionHeaders());
            app.UseCookiePolicy();
            
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseWebSockets();
            app.UseFusionSession();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapFusionWebSocketServer();
                    endpoints.MapRazorPages();
                    endpoints.MapControllers();
                    endpoints.MapHub<ClusterInfoHub>("/ClusterInfoHub");
                    endpoints.MapFallbackToFile("index.html");
                });
        }
    }
}
