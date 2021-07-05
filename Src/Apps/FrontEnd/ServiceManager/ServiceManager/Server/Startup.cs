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
using ServiceHost.Client.Shared;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.Hubs;
using ServiceManager.Shared.Api;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.Master.Commands.ServiceRegistry;
using Tauron.Host;

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
            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCompression(
                opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));
        }


        [UsedImplicitly]
        public void ConfigureContainer(IActorApplicationBuilder builder)
        {
            builder.OnMemberUp((context, system, cluster) 
                                   => ServiceRegistry.Start(system, new RegisterService(context.HostEnvironment.ApplicationName, cluster.SelfUniqueAddress, ServiceTypes.ServiceManager)))
                   .OnMemberRemoved((_, system, _) =>
                                    {
                                        var resolverScope = DependencyResolver.For(system).Resolver.CreateScope();
                                        var resolver = resolverScope.Resolver;

                                        resolver.GetService<IHubContext<ClusterInfoHub>>().Clients.All
                                                .SendAsync(HubEvents.RestartServer)
                                                .ContinueWith(_ =>
                                                              {
                                                                  using (resolverScope)
                                                                  {
                                                                      resolver.GetService<IRestartHelper>().Restart = true;
                                                                      resolver.GetService<IHostApplicationLifetime>().StopApplication();
                                                                  }
                                                              });
                                    })
                   .AddModule<MainModule>()
                   .StartNode(KillRecpientType.Frontend, IpcApplicationType.NoIpc, true);
            //.AddStateManagment(typeof(Startup).Assembly);
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

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<ClusterInfoHub>("/ClusterInfoHub");
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
