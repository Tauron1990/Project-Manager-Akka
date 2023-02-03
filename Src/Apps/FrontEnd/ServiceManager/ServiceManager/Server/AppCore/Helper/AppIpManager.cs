using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Configuration;
using Akka.Hosting;
using ServiceManager.Shared.ClusterTracking;
using Tauron.Application;

namespace ServiceManager.Server.AppCore.Helper
{
    public interface IInternalAppIpManager
    {
        IObservable<AppIp> IpChanged { get; }

        AppIp Ip { get; }
        Task<string> WriteIp(string ip);
    }

    public sealed class AppIpManager : ObservableObject, IInternalAppIpManager
    {
        private AppIp _appIpField = new("Unbekannt", IsValid: false);

        public AppIpManager()
        {
            IpChanged = PropertyChangedObservable.Where(c => c == nameof(AppIp)).Select(_ => Ip);
        }

        public IObservable<AppIp> IpChanged { get; }

        public async Task<string> WriteIp(string ip)
        {
            try
            {
                var baseConfig = ConfigurationFactory.ParseString(await File.ReadAllTextAsync("seed.conf"));
                baseConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname = {ip}").WithFallback(baseConfig);

                var targetFile = Path.Combine(TauronEnviroment.DefaultProfilePath, "servicemanager-ip.dat");

                await File.WriteAllTextAsync("seed.conf", baseConfig.ToString(includeFallback: true));
                await File.WriteAllTextAsync(targetFile, ip);

                return string.Empty;
            }
            catch (Exception e)
            {
                #pragma warning disable EPC12
                return e.Message;
                #pragma warning restore EPC12
            }
        }

        public AppIp Ip
        {
            get => _appIpField;
            private set => SetProperty(ref _appIpField, value);
        }

        public async Task Aquire()
        {
            var targetFile = Path.Combine(TauronEnviroment.DefaultProfilePath, "servicemanager-ip.dat");
            var potentialIp = await TryFindIp();
            var needPatch = false;

            if (File.Exists(targetFile))
                try
                {
                    var old = await File.ReadAllTextAsync(targetFile);
                    if (potentialIp != null && old != potentialIp)
                    {
                        needPatch = true;
                        await File.WriteAllTextAsync(targetFile, potentialIp);
                    }
                    else if (!string.IsNullOrWhiteSpace(old))
                    {
                        #if DEBUG
                        needPatch = true;
                        #endif
                        Ip = new AppIp(old, IsValid: true);
                    }
                }
                catch (IOException) { }
            else
                needPatch = true;

            if (!needPatch)
                return;

            if (string.IsNullOrWhiteSpace(potentialIp))
            {
                Ip = new AppIp("Keine Ip Gefunden Bitte manuelle Eingabe!", IsValid: false);

                return;
            }


            try
            {
                string seedPath = Path.Combine(Program.ExeFolder, AkkaConfigurationBuilder.Seed);

                var baseConfig = ConfigurationFactory.ParseString(await File.ReadAllTextAsync(seedPath));

                baseConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname = {potentialIp}").WithFallback(baseConfig);

                await File.WriteAllTextAsync(seedPath, baseConfig.ToString(includeFallback: true));

                Ip = new AppIp(potentialIp, IsValid: true);

                await File.WriteAllTextAsync(targetFile, potentialIp);
            }
            catch (Exception e)
            {
                #pragma warning disable EPC12
                Ip = new AppIp(e.Message, IsValid: false);
                #pragma warning restore EPC12
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
    }
}