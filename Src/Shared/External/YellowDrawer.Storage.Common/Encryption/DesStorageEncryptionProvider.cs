using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace YellowDrawer.Storage.Common.Encryption
{
    [PublicAPI]
    public class DesStorageEncryptionProvider : StorageEncryptionProvider
    {
        public DesStorageEncryptionProvider(string key)
        {
            EncryptionProvider = DES.Create();
            EncryptionProvider.KeySize = 256;
            EncryptionProvider.Key = Encoding.UTF8.GetBytes(key);
        }
    }
}
