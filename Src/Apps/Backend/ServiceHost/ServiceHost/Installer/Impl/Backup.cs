using System.IO;
using System.Reactive;
using Akka.Event;
using Ionic.Zip;
using Tauron;

namespace ServiceHost.Installer.Impl
{
    public sealed class Backup
    {
        private static readonly string BackupLocation = Path.GetFullPath("Backup");

        private string _backupFile = string.Empty;
        private string _backFrom = string.Empty;

        public Unit Make(string from)
        {
            BackupLocation.CreateDirectoryIfNotExis();

            _backFrom = from;
            _backupFile = Path.Combine(BackupLocation, "Backup.zip");

            using var zip = new ZipFile(_backupFile);
            zip.AddDirectory(from);
            zip.Save();

            return Unit.Default;
        }

        public Unit Recover(ILoggingAdapter log)
        {
            log.Info("Recover Old Application from Backup during Recover");

            using (var zip = ZipFile.Read(_backupFile))
                zip.ExtractAll(_backFrom, ExtractExistingFileAction.OverwriteSilently);
            
            _backupFile.DeleteFile();
            return Unit.Default;
        }

        public void CleanUp() 
            => _backupFile.DeleteFile();
    }
}