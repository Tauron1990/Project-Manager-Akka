﻿using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using SimpleHostSetup.Impl;

namespace SimpleHostSetup.DotnetBuilder
{
    public sealed class DotNetBuilder : IApplicationBuilder
    {
        private readonly ILogger _log = Log.ForContext<DotNetBuilder>();

        public async Task<bool> BuildApplication(FileInfo project, string output)
        {
            var arguments = new StringBuilder()
               .Append(" publish ")
               .Append($"\"{project.FullName}\"")
               .Append($" -o \"{output}\"")
               .Append(" -c Release")
               .Append(" -v d");

            using var process = new Process {StartInfo = new ProcessStartInfo(@"C:\Program Files\dotnet\dotnet.exe", arguments.ToString())
                                                         {
                                                             UseShellExecute = false
                                                         }};



            _log.Information("Start Project Build  Process");
            process.Start();

            await Task.Delay(1000);

            _log.Information("Wait For Exit");
            if (!process.WaitForExit(30000))
            {
                Log.Information("Killing Process");
                process.Kill(true);
            }

            _log.Information("Build Compled");
            return process.ExitCode != 0;
        }
    }
}