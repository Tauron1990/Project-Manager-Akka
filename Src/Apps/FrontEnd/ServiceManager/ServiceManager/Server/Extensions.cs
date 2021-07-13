using System;
using System.IO;
using System.Reflection;

namespace ServiceManager.Server
{
    public static class Extensions
    {
        public static string FileInAppDirectory(this string fileName)
        {
            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrWhiteSpace(basePath))
                throw new InvalidOperationException("Programm Datei nicht gefunden");

            return Path.Combine(basePath, fileName);
        }
    }
}