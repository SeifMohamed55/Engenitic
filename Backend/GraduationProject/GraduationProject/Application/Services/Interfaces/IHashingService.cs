namespace GraduationProject.Application.Services.Interfaces
{
    public interface IHashingService
    {
        Task<ulong> HashWithxxHash(Stream stream);
    }
}
