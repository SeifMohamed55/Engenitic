namespace GraduationProject.Application.Services.Interfaces
{
    public interface IEncryptionService
    {
        string AesEncrypt(string plaintext);
        string AesDecrypt(string encryptedBase64);
        string HashWithHMAC(string input);
        bool VerifyHMAC(string raw, string hash);
    }

}
