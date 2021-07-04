using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Configuration;
using ServiceManager.Shared;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace ServiceManager.Server.AppCore.Helper
{
    public sealed class AppIpManager : ObservableObject, IAppIpManager
    {
        private AppIp _appIpField = new("Unbekannt", false);

        public async Task Aquire()
        {
            var targetFile = Path.Combine(TauronEnviroment.DefaultProfilePath, "servicemanager-ip.dat");
            var potentialIp = await TryFindIp();
            var needPatch = false;

            if (File.Exists(targetFile))
            {
                try
                {
                    var old = await File.ReadAllTextAsync(targetFile);
                    if (potentialIp != null && old != potentialIp)
                    {
                        needPatch = true;
                        await File.WriteAllTextAsync(targetFile, potentialIp);
                    }
                    else if(!string.IsNullOrWhiteSpace(old))
                        Ip = new AppIp(old, true);
                }
                catch (IOException) { }
            }
            else
                needPatch = true;

            if(!needPatch)
                return;

            if (string.IsNullOrWhiteSpace(potentialIp))
            {
                Ip = new AppIp("Keine Ip Gefunden Bitte manuelle Eingabe!", false);
                return;
            }


            try
            {
                string seedPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty, AkkaConfigurationBuilder.Seed);

                var baseConfig = ConfigurationFactory.ParseString(await File.ReadAllTextAsync(seedPath));

                baseConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname = {potentialIp}").WithFallback(baseConfig);

                await File.WriteAllTextAsync(seedPath, baseConfig.ToString(true));

                Ip = new AppIp(potentialIp, true);
            }
            catch (Exception e)
            {
                Ip = new AppIp(e.Message, false);
            }
        }

        private static async Task<string?> TryFindIp()
        {
            string? localIp;
            try
            {
                using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                await socket.ConnectAsync("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint?.Address.ToString();
            }
            catch (SocketException)
            {
                localIp = null;
            }

            return localIp;
        }

        public async Task<string> WriteIp(string ip)
        {
            try
            {
                var baseConfig = ConfigurationFactory.ParseString(await File.ReadAllTextAsync("seed.conf"));
                baseConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname = {ip}").WithFallback(baseConfig);
            
                var targetFile = Path.Combine(TauronEnviroment.DefaultProfilePath, "servicemanager-ip.dat");

                await File.WriteAllTextAsync("seed.conf", baseConfig.ToString(true));
                await File.WriteAllTextAsync(targetFile, ip);

                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public AppIp Ip
        {
            get => _appIpField;
            private set => SetProperty(ref _appIpField, value);
        }
    }
}