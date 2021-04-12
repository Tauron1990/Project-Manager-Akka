using System;
using System.IO;
using YellowDrawer.Storage.Common;
using System.Linq;
using MongoDB.Driver.GridFS;
using System.Threading.Tasks;
using JetBrains.Annotations;
using YellowDrawer.Storage.Common.Encryption;

namespace YellowDrawer.Storage.GridFS
{
    [PublicAPI]
    public sealed class GridFSFileStorage : IStorageFile
    {
        private GridFSFileInfo _fileInfo;
        private IGridFSBucket _bucket;

        public GridFSFileStorage(GridFSFileInfo fileInfo, IGridFSBucket bucket)
        {
            _bucket = bucket;
            _fileInfo = fileInfo;
        }

        public string GetFileType() => _fileInfo.Filename.Split('.').ToList().Last();

        public string GetFullPath() => _fileInfo.Filename;

        public DateTime GetLastUpdated() => _fileInfo.UploadDateTime;

        public string GetName() => _fileInfo.Filename;

        public string GetPath() => _fileInfo.Filename;

        public long GetSize() => _fileInfo.Length;

        public Stream OpenRead() => _bucket.OpenDownloadStream(_fileInfo.Id);

        public Stream OpenWrite() => _bucket.OpenUploadStream(_fileInfo.Filename);

        public async Task<Stream> OpenReadAsync() => await _bucket.OpenDownloadStreamAsync(_fileInfo.Id);

        public async Task<Stream> OpenWriteAsync() => await _bucket.OpenUploadStreamAsync(_fileInfo.Filename);

        public Stream OpenCryptoRead(IStorageEncryptionProvider encryptionProvider, byte[] iv) => encryptionProvider.Decrypt(_bucket.OpenDownloadStream(_fileInfo.Id), iv);

        public Stream OpenCryptoWrite(IStorageEncryptionProvider encryptionProvider, byte[] iv) => encryptionProvider.Encrypt(_bucket.OpenUploadStream(_fileInfo.Filename), iv);
    }
}
