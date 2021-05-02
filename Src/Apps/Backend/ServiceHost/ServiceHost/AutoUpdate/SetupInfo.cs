using JetBrains.Annotations;

namespace ServiceHost.AutoUpdate
{
    [PublicAPI]
    public sealed record SetupInfo(string DownloadFile, string StartFile, string Target, int RunningProcess, int KillTime)
    {
        public string ToCommandLine() 
            => $"--downloadfile {DownloadFile} --startfile {StartFile} --target {Target} --runningprocess {RunningProcess} --killtime {KillTime}";
    }
}