using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tauron.Application.Master.Commands.Deployment.Build;

namespace ServiceManager.ProjectDeployment.Build
{
    public static class DotNetBuilder
    {
        public static Task<DeploymentErrorCode?> BuildApplication(FileInfo project, string output, Action<DeploymentMessage> log)
            => Task.Run(async () => await BuildApplicationAsync(project, output, log));

        private static async ValueTask<DeploymentErrorCode?> BuildApplicationAsync(FileInfo project, string output, Action<DeploymentMessage> log)
        {
            StringBuilder arguments = new StringBuilder()
               .Append(" publish ")
               .Append($"\"{project.FullName}\"")
               .Append($" -o \"{output}\"")
               .Append(" -c Release")
               .Append(" -v m")
               .Append(" --no-self-contained");

            var path = DedectDotNet(project);

            if (string.IsNullOrWhiteSpace(path))
                return DeploymentErrorCode.BuildDotnetNotFound;

            using var process = new Process
                                {
                                    StartInfo = new ProcessStartInfo(path, arguments.ToString())
                                                {
                                                    UseShellExecute = false,
                                                    CreateNoWindow = false,
                                                },
                                };


            log(DeploymentMessage.BuildStart);
            process.Start();

            await Task.Delay(5000);

            for (var i = 0; i < 60; i++)
            {
                await Task.Delay(2000);

                if (process.HasExited) break;
            }

            if (!process.HasExited)
            {
                log(DeploymentMessage.BuildKilling);
                process.Kill();

                return DeploymentErrorCode.BuildDotNetFailed;
            }

            log(DeploymentMessage.BuildCompled);

            return process.ExitCode == 0 ? null : DeploymentErrorCode.BuildDotNetFailed;
        }

        private static string? DedectDotNet(FileSystemInfo project)
        {
            const string netcoreapp = "netcoreapp";
            const string netstandard = "netstandard";
            const string net = "net";

            var data = XElement.Load(project.FullName);
            var frameWork = data
               .Elements("PropertyGroup")
               .SelectMany(e => e.Elements())
               .FirstOrDefault(e => e.Name == "TargetFramework")
              ?.Value;

            if (frameWork == null) return null;

            string searchTerm;

            if (frameWork.StartsWith(netcoreapp)) searchTerm = frameWork.Replace(netcoreapp, string.Empty);
            else if (frameWork.StartsWith(netstandard)) searchTerm = frameWork.Replace(netstandard, string.Empty);
            else searchTerm = frameWork.Replace(net, string.Empty);

            searchTerm = searchTerm.Substring(0, 3);

            var enviromentPaths = Environment.GetEnvironmentVariable("Path")
              ?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            var dotnetPaths = enviromentPaths?.Where(Directory.Exists).SelectMany(Directory.EnumerateFiles)
               .Where(f => f.EndsWith("dotnet.exe")).ToArray();

            return dotnetPaths?.FirstOrDefault(
                p =>
                {
                    var sdkPath = Path.Combine(Path.GetDirectoryName(p) ?? string.Empty, "sdk");

                    return Directory.Exists(sdkPath) && Directory.EnumerateDirectories(sdkPath)
                       .Any(e => new DirectoryInfo(e).Name.StartsWith(searchTerm));
                });
        }
    }
}