using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PackageTracker.Domain.Utilities
{
    public class AESKey
    {
        public string Key { get; set; }
        public string IV { get; set; }
    }

    public static class CryptoUtility
    {
        static AesCng aes = new AesCng(); // AES Crypto Next Generation

        public static AESKey CreateKey() // Crypto Key
        {
            aes.GenerateKey();
            aes.GenerateIV();
            return new AESKey
            {
                Key = Hex.ToHexString(aes.Key),
                IV = Hex.ToHexString(aes.IV)
            };
        }

        public static string CreateHashUsingMD5Async(string stringToBeHashed) // MD5 Hash
        {
            using (var md5 = MD5.Create())
            {
                byte[] hashBytes = Encoding.ASCII.GetBytes(stringToBeHashed);
                byte[] hash = md5.ComputeHash(hashBytes);
                var stringBuilder = new StringBuilder();

                for (int i = 0; i < hash.Length; i++)
                {
                    stringBuilder.Append(hash[i].ToString("X2"));
                }

                return stringBuilder.ToString();
            }
        }

        public static string Encrypt(AESKey crypto, string plainText, string salt = null)
        {
            string result = plainText;
            if (StringHelper.Exists(crypto?.Key) && StringHelper.Exists(crypto?.IV))
            {
                var encryptor = aes.CreateEncryptor(Hex.Decode(crypto.Key),Hex.Decode(crypto.IV));
                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                            if (StringHelper.Exists(salt))
                            {
                                streamWriter.Write(salt);
                            }
                        }
                        result = Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
            return result;
        }
        public static string Decrypt(AESKey crypto, string encryptedText, string salt = null)
        {
            string result = encryptedText;
            if (StringHelper.Exists(crypto?.Key) && StringHelper.Exists(crypto?.IV))
            {
                var decryptor = aes.CreateDecryptor(Hex.Decode(crypto.Key), Hex.Decode(crypto.IV));
                using (var memoryStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var streamReader = new StreamReader(cryptoStream))
                        {
                            result = streamReader.ReadToEnd();
                        }
                    }
                }
                if (StringHelper.Exists(salt))
                {
                    var n = result.LastIndexOf(salt);
                    if (n >= 0)
                        result = result.Substring(0, n);
                }
            }
            return result;
        }
    }
}
