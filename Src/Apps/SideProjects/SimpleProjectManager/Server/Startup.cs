using Akka.DependencyInjection;
using Akka.Hosting;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.ResponseCompression;
using SimpleProjectManager.Server.Controllers.ModelBinder;
using SimpleProjectManager.Server.Core.JobManager;
using SimpleProjectManager.Server.Core.Projections.Core;
using SimpleProjectManager.Server.Core.Services;
using SimpleProjectManager.Server.Core.Tasks;
using SimpleProjectManager.Shared.Services;
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
        var fusion = services.AddFusion();
        fusion
           .AddComputeService<IJobDatabaseService, JobDatabaseService>()
           .AddComputeService<IJobFileService, JobFileService>()
           .AddComputeService<ICriticalErrorService, CriticalErrorService>()
           .AddComputeService<ITaskManager, TaskManager>();

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
                                                      new IdentityModelBinderFactory()
                                                  };
                    options.ModelBinderProviders.Clear();
                    options.ModelBinderProviders.AddRange(newModelBinderProviders);
                    options.ModelBinderProviders.AddRange(oldModelBinderProviders);
                });
        services.AddResponseCompression(
            opts => opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));

        //ClientRegistrations
        //ServiceRegistrar.RegisterServices(services);
    }


    [UsedImplicitly]
    public void ConfigureContainer(IActorApplicationBuilder builder)
    {
        builder
           .ConfigureAkka((_, configurationBuilder) => configurationBuilder.AddHocon("cluster.roles = [\"Master\"]"))
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
           .RegisterStartUp<ClusterJoinSelf>(c => c.Run())
           .RegisterStartUp<JobManagerRegistrations>(jm => jm.Run())
           .RegisterStartUp<ProjectionInitializer>(i => i.Run());
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