using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;

namespace YellowDrawer.Storage.Common.Encryption
{
    [PublicAPI]
    public class Rc2StorageEncryptionProvider : StorageEncryptionProvider
    {
        public Rc2StorageEncryptionProvider(string key) => EncryptionProvider = new RC2CryptoServiceProvider {KeySize = 256, Key = Encoding.UTF8.GetBytes(key)};
    }
}
