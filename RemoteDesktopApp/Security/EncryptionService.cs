using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using RemoteDesktopApp.Configuration;

namespace RemoteDesktopApp.Security;

public class EncryptionService : IEncryptionService
{
    private readonly SecurityConfig _config;
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public EncryptionService(IOptions<SecurityConfig> config)
    {
        _config = config.Value;
        _key = Encoding.UTF8.GetBytes(_config.EncryptionKey.PadRight(32).Substring(0, 32));
        _iv = Encoding.UTF8.GetBytes(_config.EncryptionKey.PadRight(16).Substring(0, 16));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);
        
        swEncrypt.Write(plainText);
        swEncrypt.Close();
        
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            
            return srDecrypt.ReadToEnd();
        }
        catch
        {
            return string.Empty;
        }
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return string.Empty;

        using var sha256 = SHA256.Create();
        var salt = GenerateRandomBytes(16);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var saltedPassword = new byte[passwordBytes.Length + salt.Length];
        
        Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
        Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);
        
        var hash = sha256.ComputeHash(saltedPassword);
        var result = new byte[hash.Length + salt.Length];
        
        Buffer.BlockCopy(hash, 0, result, 0, hash.Length);
        Buffer.BlockCopy(salt, 0, result, hash.Length, salt.Length);
        
        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            var hashBytes = Convert.FromBase64String(hash);
            var salt = new byte[16];
            Buffer.BlockCopy(hashBytes, hashBytes.Length - 16, salt, 0, 16);
            
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltedPassword = new byte[passwordBytes.Length + salt.Length];
            
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);
            
            using var sha256 = SHA256.Create();
            var computedHash = sha256.ComputeHash(saltedPassword);
            
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != hashBytes[i])
                    return false;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateSecureKey()
    {
        return Convert.ToBase64String(GenerateRandomBytes(32));
    }

    public byte[] GenerateRandomBytes(int length)
    {
        var bytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return bytes;
    }
}