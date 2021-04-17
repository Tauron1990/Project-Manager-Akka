using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace YellowDrawer.Storage.Common.FileSystem
{
    [PublicAPI]
    public sealed class FileSystemStorageProvider : IStorageProvider
    {
        private readonly string _baseUrl;
        private readonly string _storagePath;

        public FileSystemStorageProvider(string basePath)
        {
            _storagePath = basePath;
            _baseUrl = basePath;

            _storagePath = basePath;
            if (!_baseUrl.EndsWith("/"))
                _baseUrl += '/';
        }

        private string Map(string path) => string.IsNullOrEmpty(path) ? Path.Combine(_baseUrl, _storagePath) : Path.Combine(_baseUrl, _storagePath + path);

        private static string Fix(string path) 
            => string.IsNullOrEmpty(path)
            ? string.Empty
            : Path.DirectorySeparatorChar != '/'
                ? path.Replace('/', Path.DirectorySeparatorChar)
                : path;

        #region Implementation of IStorageProvider

        public string GetPublicUrl(string path) => _baseUrl + path.Replace(Path.DirectorySeparatorChar, '/');

        public IStorageFile GetFile(string path) 
            => File.Exists(Map(path)) ? new FileSystemStorageFile(Fix(path), new FileInfo(Map(path))) 
                : throw new InvalidOperationException("File " + path + " does not exist");

        public IEnumerable<IStorageFile> ListFiles(string path)
        {
            return Directory.Exists(Map(path))
                ? new DirectoryInfo(Map(path))
                 .GetFiles()
                 .Where(fi => !IsHidden(fi))
                 .Select<FileInfo, IStorageFile>(fi => new FileSystemStorageFile(Path.Combine(Fix(path), fi.Name), fi))
                 .ToList()
                : throw new InvalidOperationException("Directory " + path + " does not exist");
        }

        public IEnumerable<IStorageFolder> ListFolders(string path)
        {
            if (Directory.Exists(Map(path)))
            {
                return new DirectoryInfo(Map(path))
                      .GetDirectories()
                      .Where(di => !IsHidden(di))
                      .Select<DirectoryInfo, IStorageFolder>(
                           di => new FileSystemStorageFolder(Path.Combine(Fix(path), di.Name), di))
                      .ToList();
            }

            try
            {
                Directory.CreateDirectory(Map(path));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"The folder could not be created at path: {path}. {ex}");
            }

            return new DirectoryInfo(Map(path))
                  .GetDirectories()
                  .Where(di => !IsHidden(di))
                  .Select<DirectoryInfo, IStorageFolder>(
                       di => new FileSystemStorageFolder(Path.Combine(Fix(path), di.Name), di))
                  .ToList();
        }

        public void CreateFolder(string path)
        {
            if (!Directory.Exists(Map(path)))
                Directory.CreateDirectory(Map(path));
            else
                throw new InvalidOperationException("Directory " + path + " already exists");
        }

        public void DeleteFolder(string path)
        {
            if (!Directory.Exists(Map(path)))
            {
                throw new InvalidOperationException("Directory " + path + " does not exist");
            }

            Directory.Delete(Map(path), true);
        }

        public void RenameFolder(string path, string newPath)
        {
            if (!Directory.Exists(Map(path)))
                throw new InvalidOperationException("Directory " + path + "does not exist");

            if (Directory.Exists(Map(newPath)))
                throw new InvalidOperationException("Directory " + newPath + " already exists");

            Directory.Move(Map(path), Map(newPath));
        }

        public IStorageFile CreateFile(string path, byte[] arr = null)
        {
            arr ??= Array.Empty<byte>();

            if (File.Exists(Map(path)))
                throw new InvalidOperationException("File " + path + " already exists");

            var fileInfo = new FileInfo(Map(path));
            File.WriteAllBytes(Map(path), arr);

            return new FileSystemStorageFile(Fix(path), fileInfo);
        }

        public bool IsFileExists(string path) => File.Exists(Map(path));

        public bool IsFolderExits(string path) => Directory.Exists(Map(path));

        public bool TryCreateFolder(string path)
        {
            try
            {
                if (Directory.Exists(Map(path))) return false;
                CreateFolder(path);
                return true;

                // return false to be consistent with FileSystemProvider's implementation
            }
            catch
            {
                return false;
            }
        }

        public void DeleteFile(string path)
        {
            if (!File.Exists(Map(path)))
                throw new InvalidOperationException("File " + path + " does not exist");

            File.Delete(Map(path));
        }

        public void RenameFile(string path, string newPath)
        {
            if (!File.Exists(Map(path)))
                throw new InvalidOperationException("File " + path + " does not exist");

            if (File.Exists(Map(newPath)))
                throw new InvalidOperationException("File " + newPath + " already exists");

            File.Move(Map(path), Map(newPath));
        }

        internal static bool IsHidden(FileSystemInfo di) => (di.Attributes & FileAttributes.Hidden) != 0;

        #endregion
    }
}