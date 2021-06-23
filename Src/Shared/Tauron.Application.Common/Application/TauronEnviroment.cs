using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;

namespace Tauron.Application
{
    [PublicAPI]
    public class TauronEnviroment : ITauronEnviroment
    {
        public static string AppRepository = "Tauron";

        private static Lazy<string> DefaultPath = new(() =>
                                                      {
                                                          var defaultPath = LocalApplicationData;
                                                          defaultPath.CreateDirectoryIfNotExis();
                                                          return defaultPath;
                                                      }, LazyThreadSafetyMode.ExecutionAndPublication);

        public static string DefaultProfilePath => DefaultPath.Value;

        private string? _defaultPath;

        string ITauronEnviroment.DefaultProfilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_defaultPath))
                    _defaultPath = DefaultPath.Value;
                return _defaultPath;
            }

            set => _defaultPath = value;
        }

        public static string LocalApplicationData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).CombinePath(AppRepository);

        string ITauronEnviroment.LocalApplicationTempFolder => LocalApplicationTempFolder;
        string ITauronEnviroment.LocalApplicationData => LocalApplicationData;

        public static string LocalApplicationTempFolder => LocalApplicationData.CombinePath("Temp");

        public IEnumerable<string> GetProfiles(string application)
        {
            return
                DefaultProfilePath.CombinePath(application)
                    .EnumerateDirectorys()
                    .Select(ent => ent.Split('\\').Last());
        }
    }
}