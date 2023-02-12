using System.IO;
using Akka.Event;
using Ionic.Zip;

namespace ServiceHost.Installer.Impl
{
    public sealed class Backup
    {
        private static readonly string BackupLocation = Path.GetFullPath("Backup");
        private string _backFrom = string.Empty;

        private string _backupFile = string.Empty;

        public void Make(string from)
        {
            if(!Directory.Exists(BackupLocation))
                Directory.CreateDirectory(BackupLocation);

            _backFrom = from;
            _backupFile = Path.Combine(BackupLocation, "Backup.zip");

            using var zip = new ZipFile(_backupFile);
            zip.AddDirectory(from);
            zip.Save();
        }

        public void Recover(ILoggingAdapter log)
        {
            log.Info("Recover Old Application from Backup during Recover");

            using (var zip = ZipFile.Read(_backupFile))
            {
                zip.ExtractAll(_backFrom, ExtractExistingFileAction.OverwriteSilently);
            }

            CleanUp();
        }

        public void CleanUp()
        {
            if(File.Exists(_backupFile))
                File.Delete(_backupFile);
        }
    }
}