namespace WorkflowAutomation.Core.Interfaces
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive data such as credentials
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts plain text using AES-256 encryption
        /// </summary>
        /// <param name="plainText">The text to encrypt</param>
        /// <returns>Encrypted text as base64 string</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// Decrypts encrypted text using AES-256 decryption
        /// </summary>
        /// <param name="cipherText">The encrypted text (base64 string)</param>
        /// <returns>Decrypted plain text</returns>
        string Decrypt(string cipherText);
    }
}
