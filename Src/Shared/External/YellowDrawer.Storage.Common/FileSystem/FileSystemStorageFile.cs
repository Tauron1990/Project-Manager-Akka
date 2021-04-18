using System;
using System.IO;
using YellowDrawer.Storage.Common.Encryption;

namespace YellowDrawer.Storage.Common.FileSystem
{
    public sealed class FileSystemStorageFile : IStorageFile
    {
        private readonly FileInfo _fileInfo;
        private readonly string _path;

        public FileSystemStorageFile(string path, FileInfo fileInfo)
        {
            _path = path;
            _fileInfo = fileInfo;
        }

        #region Implementation of IStorageFile
        public string GetFullPath() => Path.Combine(_path, _fileInfo.Name);

        public string GetPath() => _path;

        public string GetName() => _fileInfo.Name;

        public long GetSize() => _fileInfo.Length;

        public DateTime GetLastUpdated() => _fileInfo.LastWriteTime;

        public string GetFileType() => _fileInfo.Extension;

        public Stream OpenRead() => new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read);

        public Stream OpenWrite() => new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.ReadWrite);

        public Stream OpenCryptoRead(IStorageEncryptionProvider encryptionProvider, byte[] iv) => encryptionProvider.Decrypt(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read), iv);

        public Stream OpenCryptoWrite(IStorageEncryptionProvider encryptionProvider, byte[] iv) => encryptionProvider.Encrypt(new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.ReadWrite), iv);
        
        public void Delete()
        {
            if(_fileInfo.Exists)
                _fileInfo.Delete();
        }

        #endregion
    }
}
