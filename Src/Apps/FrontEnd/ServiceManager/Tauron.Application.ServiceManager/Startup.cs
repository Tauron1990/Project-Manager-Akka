using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MudBlazor.Services;
using ServiceHost.Client.Shared;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.AkkaNode.Bootstrap.Console;
using Tauron.Application.AspIntegration;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.Master.Commands.ServiceRegistry;
using Tauron.Application.Workshop;
using Tauron.Host;

namespace Tauron.Application.ServiceManager
{
    public class Startup
    {
        public Startup(IHostEnvironment environment) => Environment = environment;

        private IHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddMudServices();
        }

        [UsedImplicitly]
        public void ConfigureContainer(IActorApplicationBuilder builder)
        {
            builder.OnMemberUp((context, system, cluster) =>
                               {
                                   ServiceRegistry.Start(system, new RegisterService(context.HostEnvironment.ApplicationName, cluster.SelfUniqueAddress, ServiceTypes.ServiceManager));
                               })
               .AddModule<MainModule>()
                   .MapHostEnviroment(Environment)
                   .StartNode(KillRecpientType.Frontend, IpcApplicationType.NoIpc, true)
                   .AddStateManagment(typeof(Startup).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/Error");

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
