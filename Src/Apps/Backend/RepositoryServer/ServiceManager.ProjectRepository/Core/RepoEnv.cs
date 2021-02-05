using System;
using System.IO;
using Tauron;
using Tauron.Temp;

namespace ServiceManager.ProjectRepository.Core
{
    public static class RepoEnv
    {
        private static TempStorage? _storage;

        public static string DataPath { get; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Tauron\\ReporitoryManager");

        public static TempStorage TempFiles => _storage ??= TempStorage.CleanAndCreate(DataPath.CombinePath("Temp"));
    }
}