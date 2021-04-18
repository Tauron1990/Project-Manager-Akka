using System;
using System.IO;
using JetBrains.Annotations;
using YellowDrawer.Storage.Common.Encryption;

namespace YellowDrawer.Storage.Common
{
    [PublicAPI]
    public interface IStorageFile
    {
        string GetPath();
        string GetFullPath();
        string GetName();
        long GetSize();
        DateTime GetLastUpdated();
        string GetFileType();

        Stream OpenRead();
        Stream OpenWrite();

        Stream OpenCryptoRead(IStorageEncryptionProvider encryptionProvider, byte[] iv);
        Stream OpenCryptoWrite(IStorageEncryptionProvider encryptionProvider, byte[] iv);
        void Delete();
    }
}