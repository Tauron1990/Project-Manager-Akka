﻿using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Akka.Configuration;
using Tauron.Application.Master.Commands.Administration.Configuration;

namespace Tauron.Application.ServiceManager.AppCore.Helper
{
    public sealed class AppIpManager : IAppIpManager
    {
        private AppIp _appIp = new("Unbekannt", false);

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
                        _appIp = new AppIp(old, true);
                }
                catch (IOException) { }
            }
            else
                needPatch = true;

            if(!needPatch)
                return;

            if (string.IsNullOrWhiteSpace(potentialIp))
            {
                _appIp = new AppIp("Keine Ip Gefunden Bitte manuelle Eingabe!", false);
                return;
            }


            try
            {
                string seedPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? string.Empty, AkkaConfigurationBuilder.Seed);

                var baseConfig = ConfigurationFactory.ParseString(await File.ReadAllTextAsync(seedPath));

                baseConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname = {potentialIp}").WithFallback(baseConfig);

                await File.WriteAllTextAsync(seedPath, baseConfig.ToString(true));

                _appIp = new AppIp(potentialIp, true);
            }
            catch (Exception e)
            {
                _appIp = new AppIp(e.Message, false);
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

        public void WriteIp(string ip)
        {
            var baseConfig = ConfigurationFactory.ParseString(File.ReadAllText("seed.conf"));
            baseConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname = {ip}").WithFallback(baseConfig);
            
            var targetFile = Path.Combine(TauronEnviroment.DefaultProfilePath, "servicemanager-ip.dat");

            File.WriteAllText("seed.conf", baseConfig.ToString(true));
            File.WriteAllText(targetFile, ip);
        }

        public AppIp Ip => _appIp;
    }
}