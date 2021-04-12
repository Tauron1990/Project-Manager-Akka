using System.IO;
using System.Security.Cryptography;
using JetBrains.Annotations;

namespace YellowDrawer.Storage.Common.Encryption
{
    [PublicAPI]
    public abstract class StorageEncryptionProvider : IStorageEncryptionProvider
    {
        protected SymmetricAlgorithm EncryptionProvider;
        // ReSharper disable once InconsistentNaming
        public byte[] IV => EncryptionProvider.IV;

        // ReSharper disable once InconsistentNaming
        public byte[] GenerateIV()
        {
            EncryptionProvider.GenerateIV();
            return EncryptionProvider.IV;
        }

        public Stream Decrypt(Stream stream, byte[] iv)
        {
            EncryptionProvider.IV = iv;
            return new CryptoStream(stream, EncryptionProvider.CreateDecryptor(), CryptoStreamMode.Read);
        }

        public Stream Encrypt(Stream stream, byte[] iv)
        {
            EncryptionProvider.IV = iv;
            return new CryptoStream(stream, EncryptionProvider.CreateEncryptor(), CryptoStreamMode.Write);
        }
    }
}
