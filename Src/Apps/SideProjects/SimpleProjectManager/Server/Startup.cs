using Akka.Cluster.Hosting;
using Akka.DependencyInjection;
using Akka.Persistence;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.ResponseCompression;
using SimpleProjectManager.Server.Configuration;
using SimpleProjectManager.Server.Controllers.ModelBinder;
using SimpleProjectManager.Server.Core.Clustering;
using SimpleProjectManager.Server.Core.DeviceManager;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Server.Core.Services.Devices;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared.Services;
using SimpleProjectManager.Shared.Services.Devices;
using SimpleProjectManager.Shared.Services.Tasks;
using Stl.Collections;
using Stl.Fusion;
using Stl.Fusion.Server;
using Tauron.AkkaHost;
using Tauron.Application.AkkaNode.Bootstrap;

namespace SimpleProjectManager.Server;

public class Startup
{
    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        FusionBuilder fusion = services.AddFusion();
        fusion
           .AddComputeService<IJobDatabaseService, JobDatabaseService>()
           .AddComputeService<IJobFileService, JobFileService>()
           .AddComputeService<ICriticalErrorService, CriticalErrorService>()
           .AddComputeService<ITaskManager, TaskManager>()
           .AddComputeService<IDeviceService, DeviceService>();

        fusion.AddWebServer();

        services.AddHttpContextAccessor();
        services.AddDataProtection();

        services.Configure<HostOptions>(o => o.ShutdownTimeout = TimeSpan.FromSeconds(30));

        services.AddCors();
        services.AddSignalR();
        services.AddControllersWithViews();
        services.AddRazorPages()
           .AddMvcOptions(
                options =>
                {
                    var oldModelBinderProviders = options.ModelBinderProviders.ToList();
                    var newModelBinderProviders = new IModelBinderProvider[]
                                                  {
                                                      new IdentityModelBinderFactory(),
                                                  };
                    options.ModelBinderProviders.Clear();
                    options.ModelBinderProviders.AddRange(newModelBinderProviders);
                    options.ModelBinderProviders.AddRange(oldModelBinderProviders);
                });
        services.AddResponseCompression(
            opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));
    }


    [UsedImplicitly]
    public void ConfigureContainer(IActorApplicationBuilder builder)
    {
        StartConfigManager.ConfigManager.ConfigurateApp(builder);
        builder
           .ConfigureAkka(
                (_, configurationBuilder) =>
                {
                    configurationBuilder
                       .WithClustering(new ClusterOptions { Roles = new[] { "Master", "ProjectManager" } })
                       .WithDistributedPubSub("ProjectManager")
                       .StartActors((system, registry, resolver) 
                               => registry.Register<ClusterLogManager>(system.ActorOf(resolver.Props<ClusterLogManager>(), "ClusterLogmanager")))
                       .AddStartup((sys, _) =>
                       {
                           Persistence.Instance.Apply(sys);
                       });
                })
           .OnMemberRemoved(
                (_, system, _) =>
                {
                    try
                    {
                        using IResolverScope? resolverScope = DependencyResolver.For(system).Resolver.CreateScope();
                        IDependencyResolver? resolver = resolverScope.Resolver;
                        resolver.GetService<IHostApplicationLifetime>().StopApplication();
                    }
                    catch (ObjectDisposedException) { }
                })
           .RegisterStartUp<ClusterJoinSelf>(c => c.Run())
           .RegisterStartUp<JobManagerRegistrations>(jm => jm.Run())
           .RegisterStartUp<ProjectionInitializer>(i => i.Run())
           .RegisterStartUp<InitClientOperationController>(c => c.Run())
           .RegisterStartUp<DeviceManagerStartUp>(s => s.Run());
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseResponseCompression();

        if(env.IsDevelopment())
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

                endpoints.MapFallbackToController("NothingToSee", "Index");
                //endpoints.MapFallbackToPage("/_Host");
            });
    }
}