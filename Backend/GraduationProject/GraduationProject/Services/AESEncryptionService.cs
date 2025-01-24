namespace GraduationProject.Services
{
    using GraduationProject.StartupConfigurations;
    using Microsoft.Extensions.Options;
    using System;
    using System.Security.Cryptography;
    using System.Text;


    public interface IAesEncryptionService
    {
        string Encrypt(string plaintext);
        string Decrypt(string encryptedBase64);
    }   
    public class AesEncryptionService : IAesEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public AesEncryptionService(IOptions<JwtOptions> options)
        {
            _key = Convert.FromBase64String(options.Value.RefreshTokenKey);
            _iv = Convert.FromBase64String(options.Value.IV);
        }
        public string Encrypt(string plaintext)
        {

            if (_key.Length != 32 || _iv.Length != 16)
            {
                throw new ArgumentException("Invalid Key or IV length. Key must be 32 bytes and IV must be 16 bytes.");
            }

            // Create an AES instance
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Create an encryptor
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                // Convert plaintext to bytes
                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

                // Perform encryption
                byte[] encryptedBytes;
                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plaintextBytes, 0, plaintextBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        encryptedBytes = ms.ToArray();
                    }
                }

                // Convert encrypted bytes to Base64 string for storage
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public string Decrypt(string encryptedBase64)
        {

            // Validate key and IV lengths
            if (_key.Length != 32 || _iv.Length != 16)
            {
                throw new ArgumentException("Invalid Key or IV length. Key must be 32 bytes and IV must be 16 bytes.");
            }

            // Convert the encrypted Base64 string to a byte array
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);

            // Create an AES instance
            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Create a decryptor
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Perform decryption
                byte[] decryptedBytes;
                using (var ms = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        decryptedBytes = ms.ToArray();
                    }
                }

                // Convert decrypted bytes to string (UTF-8)
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

    }

}
