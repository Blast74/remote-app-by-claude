namespace RemoteDesktopServer.Security;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string GenerateSecureKey();
    byte[] GenerateRandomBytes(int length);
}