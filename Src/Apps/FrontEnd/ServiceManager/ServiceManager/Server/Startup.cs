using System;
using System.Linq;
using Akka.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JetBrains.Annotations;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.AppCore.ClusterTracking;
using ServiceManager.Server.AppCore.ClusterTracking.Data;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Server.AppCore.ServiceDeamon;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared;
using ServiceManager.Shared.Api;
using ServiceManager.Shared.ClusterTracking;
using ServiceManager.Shared.ServiceDeamon;
using Stl.CommandR;
using Stl.Fusion;
using Stl.Fusion.Server;
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

            services.AddCommander()
                    .AddCommandService<INodeUpdateHandler, NodeUpdateHandler>();
            
            var fusion = services.AddFusion();
            fusion.AddWebServer();
            fusion.AddComputeService<IClusterNodeTracking, ClusterNodeTracking>()
                  .AddComputeService<IClusterConnectionTracker, ClusterConnectionTracker>()
                  .AddComputeService<IServerInfo, ServerInfo>()
                  .AddComputeService<IAppIpManager, AppIpService>()
                  .AddComputeService<IDatabaseConfig, DatabaseConfig>()
                  .AddComputeService<IServerConfigurationApi, ServerConfigurationApi>();


            services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(30));
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
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

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
