using GraduationProject.Domain.Enums;
using GraduationProject.Domain.Models;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface IUploadingService
    {
        Task<FileHash?> UploadImageAsync(string imageUrl, string imageName, CloudinaryType type);
        Task<FileHash?> UploadImageAsync(Stream stream, string imageName, CloudinaryType type);
        Task<bool> ImageHashMatches(FileHash? fileHash, string imgUrl);
    }
}
