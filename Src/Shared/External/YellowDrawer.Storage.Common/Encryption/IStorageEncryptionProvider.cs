using System.IO;

namespace YellowDrawer.Storage.Common.Encryption
{
    public interface IStorageEncryptionProvider
    {
        Stream Encrypt(Stream stream, byte[] iv);
        Stream Decrypt(Stream stream, byte[] iv);
    }
}
