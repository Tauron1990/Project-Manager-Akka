using Akka.DependencyInjection;
using Akka.Persistence;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using MudBlazor.Services;
using SimpleProjectManager.Server.Core.Data;
using SimpleProjectManager.Server.Core.Projections;
using Stl.CommandR;
using Stl.Fusion;
using Stl.Fusion.Server;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap;

#pragma warning disable GU0011

namespace SimpleProjectManager.Server;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMudServices();

        var fusion = services.AddFusion();
        fusion.AddPublisher();
        
        services.AddHttpContextAccessor();
        services.AddDataProtection();

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
        builder
           .OnMemberUp(
                (_, sys, _) =>
                {
                    sys.RegisterExtension(Persistence.Instance);
                    #if DEBUG
                    TestStartUp.Run(sys);
                    #endif
                })
           .OnMemberRemoved(
                (_, system, _) =>
                {
                    try
                    {
                        using var resolverScope = DependencyResolver.For(system).Resolver.CreateScope();
                        var resolver = resolverScope.Resolver;
                        resolver.GetService<IHostApplicationLifetime>().StopApplication();
                    }
                    catch (ObjectDisposedException) { }
                })
           .AddModule<MainModule>()
           .AddModule<DataModule>()
           .AddModule<ProjectionModule>();
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
        app.UseRouting();


        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapFusionWebSocketServer();
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
    }
}