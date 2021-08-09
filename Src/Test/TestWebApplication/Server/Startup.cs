using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using Stl.Fusion;
using Stl.Fusion.Extensions;
using Stl.Fusion.Server;
using TestWebApplication.Server.Serices;
using TestWebApplication.Shared.Counter;

namespace TestWebApplication.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddFusion()
                    .AddFusionTime().AddSandboxedKeyValueStore().AddInMemoryKeyValueStore()
                    .AddComputeService<ICounterService, CounterService>()
                    .AddWebServer();
            services.Configure<ForwardedHeadersOptions>(options => {
                                                            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                                                            options.KnownNetworks.Clear();
                                                            options.KnownProxies.Clear();
                                                        });

            services.AddRouting();
            services.AddMvc().AddApplicationPart(Assembly.GetEntryAssembly());

            // Swagger & debug tools
            services.AddSwaggerGen(c => {
                                       c.SwaggerDoc("v1", new OpenApiInfo
                                                          {
                                                              Title = "Samples.Blazor.Server API",
                                                              Version = "v1"
                                                          });
                                   });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
                                    {
                                        ForwardedHeaders = ForwardedHeaders.XForwardedProto
                                    });

            app.UseWebSockets(new WebSocketOptions()
                              {
                                  KeepAliveInterval = TimeSpan.FromSeconds(30),
                              });

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSwagger();
            app.UseSwaggerUI(c => {
                                 c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                             });


            app.UseEndpoints(endpoints =>
                             {
                                 endpoints.MapFusionWebSocketServer();
                                 endpoints.MapRazorPages();
                                 endpoints.MapControllers();
                                 endpoints.MapFallbackToFile("index.html");
                             });
        }
    }
}
