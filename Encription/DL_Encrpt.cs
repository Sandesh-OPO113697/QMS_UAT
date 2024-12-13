using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace QMS.Encription
{
    public class DL_Encrpt
    {
        private readonly byte[] Key = Encoding.UTF8.GetBytes("keyforuserpasenc");
        private readonly byte[] IV = Encoding.UTF8.GetBytes("IVforuserpassenc");

        // Asynchronous Encrypt method
        public async Task<string> EncryptAsync(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            await swEncrypt.WriteAsync(plainText);  // Use WriteAsync for asynchronous operation
                        }
                    }

                    // Return the Base64 string asynchronously
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        // Asynchronous Decrypt method
        public async Task<string> DecryptAsync(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return await srDecrypt.ReadToEndAsync();  // Use ReadToEndAsync for asynchronous operation
                        }
                    }
                }
            }
        }
    }
}
