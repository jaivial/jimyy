using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using WorkflowAutomation.Core.Interfaces;

namespace WorkflowAutomation.Infrastructure.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _encryptionKey;
        private const int IvSize = 16; // 128 bits for AES

        public EncryptionService(IConfiguration configuration)
        {
            var keyString = configuration["JWT:Secret"]
                ?? configuration["ENCRYPTION_KEY"]
                ?? throw new InvalidOperationException("Encryption key not configured");

            // Ensure the key is exactly 32 bytes (256 bits) for AES-256
            _encryptionKey = DeriveKey(keyString, 32);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            using var aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var msEncrypt = new MemoryStream();

            // Write IV to the beginning of the output
            msEncrypt.Write(aes.IV, 0, aes.IV.Length);

            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                using var aes = Aes.Create();
                aes.Key = _encryptionKey;

                // Extract IV from the beginning of the cipher text
                var iv = new byte[IvSize];
                var cipher = new byte[fullCipher.Length - IvSize];

                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var msDecrypt = new MemoryStream(cipher);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);

                return srDecrypt.ReadToEnd();
            }
            catch (CryptographicException)
            {
                throw new InvalidOperationException("Failed to decrypt data. The encryption key may be incorrect.");
            }
        }

        private static byte[] DeriveKey(string password, int keySize)
        {
            using var sha256 = SHA256.Create();
            var key = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            if (key.Length != keySize)
            {
                Array.Resize(ref key, keySize);
            }

            return key;
        }
    }
}
