using System;
using System.Security.Cryptography;
using System.Text;

namespace YouRock
{
    public class CryptoHelper
    {
        public static string Sha1(string text, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            byte[] buffer = encoding.GetBytes(text);
            SHA1CryptoServiceProvider cryptoTransformSha1 = new SHA1CryptoServiceProvider();
            return BitConverter.ToString(cryptoTransformSha1.ComputeHash(buffer)).Replace("-", "");
        }

        public class Aes256
        {
            public static Tuple<string, string> CreateKeyPair(int dwKeySize = 1024, int providerType = 1)
            {
                CspParameters cspParams = new CspParameters { ProviderType = providerType };

                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(dwKeySize, cspParams);

                string publicKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(false));
                string privateKey = Convert.ToBase64String(rsaProvider.ExportCspBlob(true));

                return new Tuple<string, string>(privateKey, publicKey);
            }

            public static byte[] Encrypt(string publicKey, string data, int providerType = 1)
            {
                CspParameters cspParams = new CspParameters { ProviderType = providerType };
                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);

                rsaProvider.ImportCspBlob(Convert.FromBase64String(publicKey));

                byte[] plainBytes = Encoding.UTF8.GetBytes(data);
                byte[] encryptedBytes = rsaProvider.Encrypt(plainBytes, false);

                return encryptedBytes;
            }

            public static string Decrypt(string privateKey, byte[] encryptedBytes, int providerType = 1)
            {
                CspParameters cspParams = new CspParameters { ProviderType = providerType };
                RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);

                rsaProvider.ImportCspBlob(Convert.FromBase64String(privateKey));

                byte[] plainBytes = rsaProvider.Decrypt(encryptedBytes, false);

                string plainText = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);

                return plainText;
            }
        }
    }
}
