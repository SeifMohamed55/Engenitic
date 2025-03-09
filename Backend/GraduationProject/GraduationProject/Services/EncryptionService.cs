namespace GraduationProject.Services
{
    using GraduationProject.StartupConfigurations;
    using Microsoft.Extensions.Options;
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.IO.Hashing;


    public interface IEncryptionService
    {
        string AesEncrypt(string plaintext);
        string AesDecrypt(string encryptedBase64);
        string HashWithHMAC(string input);
        bool VerifyHMAC(string raw, string hash);
        Task<ulong> HashWithxxHash(Stream stream);

    }

    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService(IOptions<JwtOptions> options)
        {
            _key = Convert.FromBase64String(options.Value.RefreshTokenKey);
            _iv = Convert.FromBase64String(options.Value.IV);
        }
        public string AesEncrypt(string plaintext)
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

        public string AesDecrypt(string encryptedBase64)
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

        public string HashWithHMAC(string input)
        {
            using (var hmac = new HMACSHA256(_key))
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = hmac.ComputeHash(inputBytes);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyHMAC(string raw, string hash)
        {
            string newHash = HashWithHMAC(raw);
            return newHash == hash;
        }


        public async Task<ulong> HashWithxxHash(Stream file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            byte[] data = stream.ToArray(); // Get exact bytes
            ulong hash = XxHash64.HashToUInt64(data);
            return hash;
        }
    }

}
