using System;
using System.IO;
using Tauron.Application.VirtualFiles;
using Tauron.Temp;

namespace ServiceManager.ProjectRepository.Core
{
    public static class RepoEnv
    {
        private static TempStorage? _storage;

        public static string DataPath { get; } =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tauron\\ReporitoryManager");

        public static TempStorage TempFiles 
            => _storage ??= TempStorage.CleanAndCreate(VirtualFileFactory.Shared.Local(Path.Combine(DataPath, "Temp")));
    }
}