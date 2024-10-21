namespace Backend.Security;

public interface IAESEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
