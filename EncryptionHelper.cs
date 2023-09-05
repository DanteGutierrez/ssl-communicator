using System.Security.Cryptography;
using System.Text;

public class EncryptionHelper
{
    /// <summary>
    /// Encrypts a cleartext message
    /// </summary>
    /// <param name="cleartextMessage">The message to be encrypted</param>
    /// <param name="publicKey">The public key</param>
    /// <returns>A base64 encrypted message</returns>
    public static string RSAEncryptToBase64(string cleartextMessage, string publicKey)
    {
        byte[] data = Encoding.Unicode.GetBytes(cleartextMessage);

        byte[] encrypted;

        string base64;

        using (RSA rsa = RSA.Create())
        {
            rsa.ImportFromPem(publicKey);

            encrypted = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);

            base64 = Convert.ToBase64String(encrypted);
        }

        return base64;
    }

    /// <summary>
    /// Decrypts an encrypted message
    /// </summary>
    /// <param name="encryptedBase64">The base64 of the encrypted message</param>
    /// <param name="privateKeyFileName">The file path to the private key</param>
    /// <returns>A cleartext message</returns>
    public static string RSADecryptFromBase64(string encryptedBase64, string privateKeyFileName)
    {
        byte[] encrypted = Convert.FromBase64String(encryptedBase64);

        byte[] decrypted;

        string message;

        using (RSA rsa = RSA.Create())
        {
            try
            {
                rsa.ImportFromPem(File.ReadAllText(privateKeyFileName).Trim());
            }
            catch (Exception)
            {
                string password = ConsoleHelper.GetResponse($"DECRYPTING: Enter private key password for: {privateKeyFileName}");

                rsa.ImportFromEncryptedPem(File.ReadAllText(privateKeyFileName).Trim(), password);
            }

            decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.Pkcs1);

            message = Encoding.Unicode.GetString(decrypted);
        }

        return message;
    }

    /// <summary>
    /// Creates a signature from a message and private key
    /// </summary>
    /// <param name="cleartextMessage">The message</param>
    /// <param name="privateKeyFileName">The file path to the private key</param>
    /// <returns>The base64 of the signature</returns>
    public static string SignData(string cleartextMessage, string privateKeyFileName)
    {
        byte[] data = Encoding.Unicode.GetBytes(cleartextMessage);

        string signature;

        using (RSA rsa = RSA.Create())
        {
            try
            {
                rsa.ImportFromPem(File.ReadAllText(privateKeyFileName).Trim());
            }
            catch (Exception)
            {
                string password = ConsoleHelper.GetResponse($"SIGNING: Enter private key password for: {privateKeyFileName}");

                rsa.ImportFromEncryptedPem(File.ReadAllText(privateKeyFileName).Trim(), password);
            }

            byte[] signedHash = rsa.SignData(data, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

            signature = Convert.ToBase64String(signedHash);
        }

        return signature;
    }

    /// <summary>
    /// Verifies a decrypted message matches the signature
    /// </summary>
    /// <param name="decryptedCleartextMessage">The message that was decrypted</param>
    /// <param name="publicKey">The public key</param>
    /// <param name="base64signature">The base64 of the signature</param>
    /// <returns>True for a valid verification, false for invalid</returns>
    public static bool VerifyData(string decryptedCleartextMessage, string publicKey, string base64signature)
    {
        string data = decryptedCleartextMessage.Trim();

        bool valid;

        using (RSA rsa = RSA.Create())
        {
            rsa.ImportFromPem(publicKey.Trim());

            valid = rsa.VerifyData(Encoding.Unicode.GetBytes(data), Convert.FromBase64String(base64signature), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        }

        return valid;
    }

    /// <summary>
    /// Takes in a cleartext password and turns it into an AES Key
    /// </summary>
    /// <param name="password">Cleartext Password</param>
    /// <returns>Base64 Key</returns>
    public static string GenerateKeyFromPassword(string password)
    {
        byte[] key;

        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));

            key = new byte[16];

            for (int i = 0; i < key.Length; i++)
            {
                key[i] = hash[i];
            }
        }

        string result = Convert.ToBase64String(key);

        return result;
    }

    /// <summary>
    /// Encrypts text with a base64 key
    /// </summary>
    /// <param name="text">Cleartext</param>
    /// <param name="key">Base64 key</param>
    /// <returns>Base64 of encrypted cleartext</returns>
    public static string AESEncrypt(string text, string key)
    {
        byte[] cypher;

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 128;
            aes.Key = Convert.FromBase64String(key);
            aes.BlockSize = 128;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encrypter = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encrypter, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(text);
                    }
                    cypher = ms.ToArray();
                }
            }
        }

        string result = Convert.ToBase64String(cypher);

        return result;
    }

    /// <summary>
    /// Takes base64 encryption and decrypts against a base64 key
    /// </summary>
    /// <param name="cypher">Base64 of encryption</param>
    /// <param name="key">Base64 of Key</param>
    /// <returns>Cleartext decrypted result</returns>
    public static string AESDecrypt(string cypher, string key)
    {
        string text;

        using (Aes aes = Aes.Create())
        {
            aes.KeySize = 128;
            aes.Key = Convert.FromBase64String(key);
            aes.BlockSize = 128;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decrypter = aes.CreateDecryptor(aes.Key, aes.IV);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cypher)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decrypter, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        text = sr.ReadToEnd();
                    }
                }
            }
        }
        return text;
    }
}