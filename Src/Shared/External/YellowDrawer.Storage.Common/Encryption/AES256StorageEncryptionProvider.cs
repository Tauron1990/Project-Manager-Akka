using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace YellowDrawer.Storage.Common.Encryption
{
    [PublicAPI]
    public class AesStorageEncryptionProvider : StorageEncryptionProvider
    {
        public AesStorageEncryptionProvider(string key)
        {
            EncryptionProvider = new AesCryptoServiceProvider
            {
                KeySize = 256,
                Key = Encoding.UTF8.GetBytes(key)
            };
        }

        
    }
}
