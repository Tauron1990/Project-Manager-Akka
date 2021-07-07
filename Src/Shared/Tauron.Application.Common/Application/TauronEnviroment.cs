using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Application
{
    internal sealed class TauronEnviromentImpl : ITauronEnviroment
    {
        private string? _defaultPath;

        string ITauronEnviroment.DefaultProfilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_defaultPath))
                    _defaultPath = TauronEnviroment.DefaultPath.Value;
                return _defaultPath;
            }

            set => _defaultPath = value;
        }

        public string LocalApplicationData { get; }

        public string LocalApplicationTempFolder { get; }

        public IEnumerable<string> GetProfiles(string application)
        {
            return TauronEnviroment.DefaultProfilePath.CombinePath(application)
                                   .EnumerateDirectorys()
                                   .Select(ent => ent.Split('\\').Last());
        }

        public TauronEnviromentImpl()
        {
            LocalApplicationData = TauronEnviroment.LocalApplicationData;
            LocalApplicationTempFolder = TauronEnviroment.LocalApplicationTempFolder;
        }
    }

    [PublicAPI]
    public static class TauronEnviroment
    {
        public static string AppRepository = "Tauron";

        internal static Lazy<string> DefaultPath = new(() =>
                                                      {
                                                          var defaultPath = LocalApplicationData;
                                                          defaultPath.CreateDirectoryIfNotExis();
                                                          return defaultPath;
                                                      }, LazyThreadSafetyMode.ExecutionAndPublication);

        public static string DefaultProfilePath => DefaultPath.Value;

        public static string LocalApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).CombinePath(AppRepository);

        public static string LocalApplicationTempFolder => LocalApplicationData.CombinePath("Temp");
    }
}