using System.Security.Cryptography;
using System.Text;

namespace Commun.Crypto
{
    public static class EncryptionHelper
    {
        private const string EncryptionKey
            = "nk5T5ZE3UCGGiHqvkPUuhWwR15PQmgDPwLWMslaFchlpR1QKDfOX1VfcuoDfUPmg2t9QZwguIkA4pztBXcpogUrmtnUx1xbhHR95tUjuV20zce3AUZEDu9m";

        public static string Encrypt(this string clearText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x32, 0x76, 0x61, 0x6e, 0x20, 0x4a, 0x65, 0x64, 0x16, 0x65, 0x64, 0x65, 0x32 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(this string cipherText)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x32, 0x76, 0x61, 0x6e, 0x20, 0x4a, 0x65, 0x64, 0x16, 0x65, 0x64, 0x65, 0x32 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}
