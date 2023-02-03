using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using ServiceManager.ProjectDeployment.Data;
using ServiceManager.Server.AppCore;
using ServiceManager.Server.AppCore.Helper;
using ServiceManager.Server.AppCore.Identity;
using ServiceManager.Shared.Identity;
using Tauron.Application.AkkaNode.Bootstrap;
using Tauron.Application.Master.Commands.KillSwitch;
using Tauron.Application.MongoExtensions;

namespace ServiceManager.Server
{
    public static class Program
    {
        //<!--@(await Html.RenderComponentAsync<App>(RenderMode.WebAssemblyPrerendered, new { SessionId = sessionId }))-->

        public static readonly string ExeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty;

        private static readonly RestartHelper RestartHelper = new();
        private static readonly AppIpManager AppIpManager = new();

        public static async Task Main(string[] args)
        {
            ImmutableListSerializer<Condition>.Register();
            ImmutableListSerializer<SeedUrl>.Register();
            ImmutableListSerializer<AppFileInfo>.Register();

            await AppIpManager.Aquire();
            var host = CreateHostBuilder(args).Build();

            await MigrateDatabase(host.Services);

            await host.RunAsync();

            // ReSharper disable once RedundantJumpStatement
            if (!RestartHelper.Restart) return;

            #if RELSEASE
            var file = Path.ChangeExtension(Assembly.GetEntryAssembly()?.Location, ".exe");
            if (!string.IsNullOrWhiteSpace(file) && File.Exists(file))
                Process.Start(file);

            #endif
        }

        private static async Task MigrateDatabase(IServiceProvider provider)
        {
            using var scope = provider.CreateScope();

            await using var context = scope.ServiceProvider.GetRequiredService<UsersDatabase>();
            await context.Database.MigrateAsync();

            using var manager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var claim in Claims.AllClaims)
            {
                if (await manager.RoleExistsAsync(claim)) continue;

                await manager.CreateAsync(new IdentityRole(claim));
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
               .ConfigureServices(sc => sc.AddSingleton<IRestartHelper>(RestartHelper).AddSingleton<IInternalAppIpManager>(AppIpManager))
               .StartNode(KillRecpientType.Frontend, IpcApplicationType.NoIpc, consoleLog: true)
               .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        if (AppIpManager.Ip.IsValid)
                            webBuilder.UseUrls("http://localhost:83"); //, $"http://{AppIpManager.Ip.Ip}:81");
                        else
                            webBuilder.UseUrls("http://localhost:83");
                        webBuilder.UseStartup<Startup>();
                    });
    }
}